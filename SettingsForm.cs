using System;
using System.Windows.Forms;

namespace FtpClientLab30
{
    public class SettingsForm : Form
    {
        private readonly TextBox tbHost = new TextBox();
        private readonly TextBox tbUser = new TextBox();
        private readonly TextBox tbPass = new TextBox();
        private readonly CheckBox cbShort = new CheckBox();
        private readonly Button btnSave = new Button();

        public ClientSettings Settings { get; private set; }

        public SettingsForm(ClientSettings settings)
        {
            Settings = settings;
            Text = "Налаштування FTP клієнта";
            Width = 430;
            Height = 260;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            var lblHost = new Label { Text = "FTP хост:", Left = 20, Top = 25, Width = 130 };
            tbHost.SetBounds(160, 22, 220, 24);
            tbHost.Text = settings.Host;

            var lblUser = new Label { Text = "Ім'я користувача:", Left = 20, Top = 65, Width = 130 };
            tbUser.SetBounds(160, 62, 220, 24);
            tbUser.Text = settings.User;

            var lblPass = new Label { Text = "Пароль:", Left = 20, Top = 105, Width = 130 };
            tbPass.SetBounds(160, 102, 220, 24);
            tbPass.Text = settings.Password;
            tbPass.UseSystemPasswordChar = true;

            cbShort.Text = "Скорочене відображення TreeView";
            cbShort.SetBounds(160, 140, 240, 24);
            cbShort.Checked = settings.ShortView;

            btnSave.Text = "Зберегти";
            btnSave.SetBounds(160, 175, 120, 32);
            btnSave.Click += BtnSave_Click;

            Controls.AddRange(new Control[] { lblHost, tbHost, lblUser, tbUser, lblPass, tbPass, cbShort, btnSave });
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            Settings.Host = tbHost.Text.Trim();
            Settings.User = tbUser.Text.Trim();
            Settings.Password = tbPass.Text;
            Settings.ShortView = cbShort.Checked;
            Settings.Save();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
