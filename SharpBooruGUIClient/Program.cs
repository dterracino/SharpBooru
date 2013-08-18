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

            bool cmdLineCreds = args.Length == 4;
            string username = null, password = null;
            if (cmdLineCreds)
            {
                username = args[2];
                password = args[3];
            }

            Booru booru = null;
            try
            {
                if (cmdLineCreds)
                {
                    booru = new Booru(server, port, username, password);
                    booru.Connect();
                }
                else for (int tryCount = 0; tryCount < 3; tryCount++)
                        try
                        {
                            if (!LoginDialog.ShowDialog(ref username, ref password))
                            {
                                if (booru != null)
                                    booru.Dispose();
                                return 1; //maybe return 0?
                            }
                            else
                            {
                                booru = new Booru(server, port, username, password);
                                booru.Connect();
                                break;
                            }
                        }
                        catch (BooruProtocol.BooruException bEx)
                        {
                            if (booru != null)
                                booru.Dispose();
                            booru = null;
                            if (bEx.ErrorCode != BooruProtocol.ErrorCode.LoginFailed)
                                throw;
                            else MessageBox.Show("Login failed", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                if (booru != null)
                    using (MainForm form = new MainForm(booru))
                        Application.Run(form);
                else throw new BooruProtocol.BooruException(BooruProtocol.ErrorCode.LoginFailed);
            }
            catch (Exception ex)
            {
                string exName = ex.GetType().Name;
                string msg = string.Format("{0}: {1}", exName, ex.Message);
                MessageBox.Show(msg, exName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }
            finally
            {
                if (booru != null)
                    booru.Dispose();
                Helper.CleanTempFolder();
            }

            return 0;
        }
    }
}