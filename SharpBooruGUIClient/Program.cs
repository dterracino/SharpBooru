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

            string username = null, password = null;
            if (args.Length == 4)
            {
                username = args[2];
                password = args[3];
            }
            else if (!LoginDialog.ShowDialog(ref username, ref password))
                return 1;

            if (username != null)
                try
                {
                    using (Booru booru = new Booru(server, port, username, password))
                    using (MainForm form = new MainForm(booru))
                        Application.Run(form);
                }
                catch (Exception ex)
                {
                    string exName = ex.GetType().Name;
                    string msg = string.Format("{0}: {1}", exName, ex.Message);
                    MessageBox.Show(msg, exName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 1;
                }

            return 0;
        }
    }
}