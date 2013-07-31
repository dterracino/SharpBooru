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

            using (LoginDialog lDialog  =new LoginDialog())
                if (lDialog.ShowDialog() == DialogResult.OK)
                {
                    string username = lDialog.Username;
                    string password = lDialog.Password;

                    using (Booru booru = new Booru(server, 2400, username, password))
                        Application.Run(new MainForm(booru));
                }

            return 0;
        }
    }
}
