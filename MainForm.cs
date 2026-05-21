using System;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace FtpClientLab30
{
    public class MainForm : Form
    {
        private TextBox tbHost = new TextBox();
        private TextBox tbUser = new TextBox();
        private TextBox tbPass = new TextBox();
        private TextBox tbPath = new TextBox();
        private TextBox tbNewName = new TextBox();
        private TextBox tbLocal = new TextBox();
        private TreeView ftpTree = new TreeView();
        private ListBox logBox = new ListBox();
        private CheckBox cbShort = new CheckBox();
        private ClientSettings settings;

        public MainForm()
        {
            settings = ClientSettings.Load();
            BuildInterface();
            LoadSettingsToForm();
        }

        private void BuildInterface()
        {
            Text = "FTP Client - Lab 30";
            Width = 1040;
            Height = 650;
            StartPosition = FormStartPosition.CenterScreen;

            Label l1 = new Label { Text = "Хост:", Left = 15, Top = 18, Width = 120 };
            tbHost.SetBounds(140, 15, 260, 24);

            Label l2 = new Label { Text = "Користувач:", Left = 15, Top = 50, Width = 120 };
            tbUser.SetBounds(140, 47, 260, 24);

            Label l3 = new Label { Text = "Пароль:", Left = 15, Top = 82, Width = 120 };
            tbPass.SetBounds(140, 79, 260, 24);
            tbPass.UseSystemPasswordChar = true;

            cbShort.Text = "Скорочений вигляд";
            cbShort.SetBounds(140, 110, 220, 24);

            Button btnConnect = MakeButton("LIST / Підключитись", 420, 15, ConnectAndList);
            Button btnNlist = MakeButton("NLIST", 420, 50, (s, e) => ExecuteList(WebRequestMethods.Ftp.ListDirectory));
            Button btnSettings = MakeButton("Налаштування", 420, 85, OpenSettings);

            Label l4 = new Label { Text = "Шлях на FTP:", Left = 15, Top = 150, Width = 120 };
            tbPath.SetBounds(140, 147, 260, 24);
            tbPath.Text = "/";

            Label l5 = new Label { Text = "Нове ім'я:", Left = 15, Top = 182, Width = 120 };
            tbNewName.SetBounds(140, 179, 260, 24);

            Label l6 = new Label { Text = "Локальний файл/папка:", Left = 15, Top = 214, Width = 125 };
            tbLocal.SetBounds(140, 211, 260, 24);
            Button btnFile = MakeButton("Обрати файл", 420, 207, ChooseFile);
            Button btnFolder = MakeButton("Обрати папку", 550, 207, ChooseFolder);

            GroupBox group = new GroupBox { Text = "FTP команди", Left = 15, Top = 250, Width = 655, Height = 180 };
            string[] names = { "SIZE", "MDTM", "RETR", "STOR", "STOU", "APPE", "DELE", "MKD", "RMD", "RENAME", "Група файлів", "Папка з файлами" };
            EventHandler[] actions = {
                (s,e)=>ExecuteSimple(WebRequestMethods.Ftp.GetFileSize),
                (s,e)=>ExecuteSimple(WebRequestMethods.Ftp.GetDateTimestamp),
                DownloadFile,
                UploadFile,
                UploadUnique,
                AppendFile,
                (s,e)=>ExecuteSimple(WebRequestMethods.Ftp.DeleteFile),
                (s,e)=>ExecuteSimple(WebRequestMethods.Ftp.MakeDirectory),
                (s,e)=>ExecuteSimple(WebRequestMethods.Ftp.RemoveDirectory),
                RenameItem,
                UploadManyFiles,
                UploadFolder
            };
            for (int i = 0; i < names.Length; i++)
            {
                Button b = new Button { Text = names[i], Left = 15 + (i % 4) * 155, Top = 25 + (i / 4) * 45, Width = 140, Height = 32 };
                b.Click += actions[i];
                group.Controls.Add(b);
            }

            ftpTree.SetBounds(690, 15, 320, 415);
            logBox.SetBounds(15, 445, 995, 155);

            Controls.AddRange(new Control[] { l1, tbHost, l2, tbUser, l3, tbPass, cbShort, btnConnect, btnNlist, btnSettings, l4, tbPath, l5, tbNewName, l6, tbLocal, btnFile, btnFolder, group, ftpTree, logBox });
        }

        private Button MakeButton(string text, int x, int y, EventHandler click)
        {
            Button b = new Button { Text = text, Left = x, Top = y, Width = 120, Height = 30 };
            b.Click += click;
            return b;
        }

        private void LoadSettingsToForm()
        {
            tbHost.Text = settings.Host;
            tbUser.Text = settings.User;
            tbPass.Text = settings.Password;
            cbShort.Checked = settings.ShortView;
        }

        private string HostRoot => tbHost.Text.EndsWith("/") ? tbHost.Text : tbHost.Text + "/";
        private string FtpPath => tbPath.Text.TrimStart('/');
        private string FullUrl => HostRoot + FtpPath;

        private FtpWebRequest CreateRequest(string url, string method)
        {
            var request = (FtpWebRequest)WebRequest.Create(url);
            request.Credentials = new NetworkCredential(tbUser.Text, tbPass.Text);
            request.Method = method;
            request.UseBinary = true;
            request.KeepAlive = false;
            return request;
        }

        private void Log(string text) => logBox.Items.Add(DateTime.Now.ToString("HH:mm:ss") + "  " + text);

        private void ConnectAndList(object? sender, EventArgs e) => ExecuteList(WebRequestMethods.Ftp.ListDirectoryDetails);

        private void ExecuteList(string method)
        {
            try
            {
                ftpTree.Nodes.Clear();
                var request = CreateRequest(FullUrl, method);
                using var response = (FtpWebResponse)request.GetResponse();
                using var reader = new StreamReader(response.GetResponseStream()!);
                TreeNode root = new TreeNode(HostRoot);
                ftpTree.Nodes.Add(root);

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine() ?? "";
                    string nodeText = cbShort.Checked ? GetShortName(line) : line;
                    root.Nodes.Add(nodeText);
                }
                root.Expand();
                Log("Список FTP отримано: " + response.StatusDescription);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private string GetShortName(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return line;
            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[^1] : line;
        }

        private void ExecuteSimple(string method)
        {
            try
            {
                var request = CreateRequest(FullUrl, method);
                using var response = (FtpWebResponse)request.GetResponse();
                Log(method + ": " + response.StatusDescription);
                MessageBox.Show(response.StatusDescription);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void ChooseFile(object? sender, EventArgs e)
        {
            using OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == DialogResult.OK) tbLocal.Text = dlg.FileName;
        }

        private void ChooseFolder(object? sender, EventArgs e)
        {
            using FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK) tbLocal.Text = dlg.SelectedPath;
        }

        private void DownloadFile(object? sender, EventArgs e)
        {
            try
            {
                using SaveFileDialog dlg = new SaveFileDialog { FileName = Path.GetFileName(FtpPath) };
                if (dlg.ShowDialog() != DialogResult.OK) return;
                var request = CreateRequest(FullUrl, WebRequestMethods.Ftp.DownloadFile);
                using var response = (FtpWebResponse)request.GetResponse();
                using var stream = response.GetResponseStream();
                using var file = File.Create(dlg.FileName);
                stream!.CopyTo(file);
                Log("RETR: файл завантажено");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void UploadFile(object? sender, EventArgs e) => UploadByMethod(WebRequestMethods.Ftp.UploadFile);
        private void UploadUnique(object? sender, EventArgs e) => UploadByMethod(WebRequestMethods.Ftp.UploadFileWithUniqueName);
        private void AppendFile(object? sender, EventArgs e) => UploadByMethod(WebRequestMethods.Ftp.AppendFile);

        private void UploadByMethod(string method)
        {
            try
            {
                if (!File.Exists(tbLocal.Text)) { MessageBox.Show("Оберіть локальний файл"); return; }
                string url = HostRoot + FtpPath.TrimEnd('/') + "/" + Path.GetFileName(tbLocal.Text);
                var request = CreateRequest(url, method);
                byte[] data = File.ReadAllBytes(tbLocal.Text);
                using var reqStream = request.GetRequestStream();
                reqStream.Write(data, 0, data.Length);
                using var response = (FtpWebResponse)request.GetResponse();
                Log(method + ": " + response.StatusDescription);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void RenameItem(object? sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tbNewName.Text)) { MessageBox.Show("Введіть нове ім'я"); return; }
                var request = CreateRequest(FullUrl, WebRequestMethods.Ftp.Rename);
                request.RenameTo = tbNewName.Text;
                using var response = (FtpWebResponse)request.GetResponse();
                Log("RENAME: " + response.StatusDescription);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void UploadManyFiles(object? sender, EventArgs e)
        {
            using OpenFileDialog dlg = new OpenFileDialog { Multiselect = true };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            foreach (string file in dlg.FileNames)
            {
                tbLocal.Text = file;
                UploadByMethod(WebRequestMethods.Ftp.UploadFile);
            }
            Log("Групу файлів завантажено");
        }

        private void UploadFolder(object? sender, EventArgs e)
        {
            try
            {
                if (!Directory.Exists(tbLocal.Text)) { MessageBox.Show("Оберіть папку"); return; }
                foreach (string file in Directory.GetFiles(tbLocal.Text))
                {
                    string url = HostRoot + FtpPath.TrimEnd('/') + "/" + Path.GetFileName(file);
                    var request = CreateRequest(url, WebRequestMethods.Ftp.UploadFile);
                    byte[] data = File.ReadAllBytes(file);
                    using var reqStream = request.GetRequestStream();
                    reqStream.Write(data, 0, data.Length);
                    using var response = (FtpWebResponse)request.GetResponse();
                    Log("Завантажено: " + Path.GetFileName(file));
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void OpenSettings(object? sender, EventArgs e)
        {
            settings.Host = tbHost.Text;
            settings.User = tbUser.Text;
            settings.Password = tbPass.Text;
            settings.ShortView = cbShort.Checked;
            using var form = new SettingsForm(settings);
            if (form.ShowDialog() == DialogResult.OK)
            {
                settings = form.Settings;
                LoadSettingsToForm();
                Log("Налаштування збережено у ftp_settings.txt");
            }
        }
    }
}
