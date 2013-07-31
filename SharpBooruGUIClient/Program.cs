using System;
using System.Windows.Forms;

namespace TA.SharpBooru.Client.GUI
{
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string server = args.Length > 0 ? args[0] : "127.0.01";
            ushort port = args.Length > 1 ? Convert.ToUInt16(args[1]) : (ushort)2400;

            string username, password;
            if (args.Length == 4)
            {
                username = args[2];
                password = args[3];
            }
            else ShowLoginDialog(out username, out password);

            if (username != null)
                using (Booru booru = new Booru(server, port, username, password))
                    Application.Run(new MainForm(booru));

            return 0;
        }

        private static bool ShowLoginDialog(out string Username, out string Password)
        {
            using (LoginDialog lDialog = new LoginDialog())
                if (lDialog.ShowDialog() == DialogResult.OK)
                {
                    Username = lDialog.Username;
                    Password = lDialog.Password;
                    return true;
                }
                else
                {
                    Username = null;
                    Password = null;
                    return false;
                }
        }
    }
}