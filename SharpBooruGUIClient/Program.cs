using System;
using System.Net;
using System.Windows.Forms;
using CommandLine;

namespace TA.SharpBooru.Client.GUI
{
    public class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            Options options = new Options();
            try
            {
                if (Parser.Default.ParseArguments(args, options))
                {
                    (new Program()).Run(options);
                    return 0;
                }
            }
            catch (Exception ex) { TryHandleException(ex); }
            return 1;
        }

        public static void TryHandleException(Exception ex)
        {
            string exName = ex.GetType().Name;
            string exMsg = string.Format("{0}: {1}", exName, ex.Message);
            try { MessageBox.Show(exMsg, exName, MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch { Console.WriteLine(exMsg); }
        }

        public void Run(Options options)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            IPEndPoint endPoint = Helper.GetIPEndPointFromString(options.Server);
            bool usernameSpecified = !string.IsNullOrWhiteSpace(options.Username);
            bool passwordSpecified = !string.IsNullOrEmpty(options.Password);

            Booru booru = null;
            if (usernameSpecified && passwordSpecified)
            {
                booru = new Booru(endPoint, options.Username, options.Password);
            }
            else
            {
                string username = options.Username;
                string password = string.Empty; //Don't fill in password automatically
                if (LoginDialog.ShowDialog(ref username, ref password))
                    booru = new Booru(endPoint, username, password);
                else throw new Exception("Login procedure not completed");
            }

            try { ShowBooru(booru); }
            finally
            {
                booru.Dispose();
                Helper.CleanTempFolder();
            }
        }

        public void ShowBooru(Booru Booru)
        {
            using (MainForm mForm = new MainForm(Booru))
                Application.Run(mForm);
        }
    }
}