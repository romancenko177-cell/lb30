using System;
using System.IO;

namespace FtpClientLab30
{
    public class ClientSettings
    {
        public string Host { get; set; } = "ftp://127.0.0.1/";
        public string User { get; set; } = "user";
        public string Password { get; set; } = "";
        public bool ShortView { get; set; } = false;

        public static string FilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ftp_settings.txt");

        public static ClientSettings Load()
        {
            var settings = new ClientSettings();
            if (!File.Exists(FilePath)) return settings;

            foreach (var line in File.ReadAllLines(FilePath))
            {
                var parts = line.Split('=', 2);
                if (parts.Length != 2) continue;
                switch (parts[0])
                {
                    case "Host": settings.Host = parts[1]; break;
                    case "User": settings.User = parts[1]; break;
                    case "Password": settings.Password = parts[1]; break;
                    case "ShortView": settings.ShortView = parts[1] == "True"; break;
                }
            }
            return settings;
        }

        public void Save()
        {
            File.WriteAllLines(FilePath, new[]
            {
                "Host=" + Host,
                "User=" + User,
                "Password=" + Password,
                "ShortView=" + ShortView
            });
        }
    }
}
