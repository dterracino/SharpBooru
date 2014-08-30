using System;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using TA.SharpBooru.Client;
using TA.SharpBooru.Server;
using TA.SharpBooru.Client.GUI;
using TA.SharpBooru.Client.CLI;
using TA.SharpBooru.NetIO.Encryption;
using TA.SharpBooru.Server.WebServer;
using TA.SharpBooru.Server.WebServer.VFS;
using CommandLine;

namespace TA.SharpBooru
{
    public class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            Console.Title = "SharpBooru";
            Console.WriteLine(Helper.IsWindows() ? Properties.Resources.ascii_banner_windows : Properties.Resources.ascii_banner_unix);
            Console.WriteLine();

            Logger sLogger = new Logger(Console.Out);
            Options options = new Options();
            try
            {
                if (Parser.Default.ParseArguments(args, options))
                {
                    if (options.Mode == Options.RunMode.Server)
                    {
                        Console.Title = "SharpBooru Server";
                        if (Helper.IsWindows())
                            Console.WindowWidth = 120;
                        ServerWrapper wrapper = new ServerWrapper(sLogger);
                        wrapper.StartServer(options.Location);
                    }
                    else if (options.Mode == Options.RunMode.GUI)
                    {
                        BooruClient.CheckFingerprintDelegate fpChecker = fp =>
                            {
                                string question = string.Format("Server fingerprint = {0}, accept?", fp);
                                return MessageBox.Show(question, "Fingerprint", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
                            };
                        using (BooruClient booru = ConnectBooru(sLogger, options.Server, options.AcceptFingerprint ? null : fpChecker, options))
                            RunClientGUI(booru);
                    }
                    else if (options.Mode == Options.RunMode.CLI)
                    {
                        BooruClient.CheckFingerprintDelegate fpChecker = fp =>
                            {
                                while (true)
                                {
                                    Console.Write("Server fingerprint = {0}, accept? [Y/N] ", fp);
                                    ConsoleKey key = Console.ReadKey().Key;
                                    Console.WriteLine();
                                    if (key == ConsoleKey.Y)
                                        return true;
                                    else if (key == ConsoleKey.N)
                                        return false;
                                }
                            };
                        using (BooruClient booru = ConnectBooru(sLogger, options.Server, options.AcceptFingerprint ? null : fpChecker, options))
                            RunClientCLI(booru, options.Command);
                    }
                    else return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                sLogger.LogException("SharpBooru", ex);
                return 1;
            }
            finally { Helper.CleanTempFolder(); }
        }

        private static BooruClient ConnectBooru(Logger Logger, IPEndPoint endPoint, BooruClient.CheckFingerprintDelegate fpChecker, Options loginOptions)
        {
            BooruClient client = new BooruClient(Logger);
            client.Connect(endPoint, fpChecker);
            if (loginOptions.Keypair != null)
                using (RSA rsa = new RSA(loginOptions.Keypair))
                    client.Login(rsa);
            else if (loginOptions.Username != null && loginOptions.Password != null)
                client.Login(loginOptions.Username, loginOptions.Password);
            return client;
        }

        private static void RunClientGUI(BooruClient booru)
        {
            //TODO Show connect dialog when connection string not given
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (MainForm mForm = new MainForm(booru))
            {
                GUIHelper.ToggleConsoleWindow();
                mForm.ShowDialog();
            }
        }

        private static void RunClientCLI(BooruClient booru, string command = null)
        {
            Console.Title = "SharpBooru CLI Client";

            BooruConsole bConsole = new BooruConsole(booru);
            if (!string.IsNullOrWhiteSpace(command))
                bConsole.ExecuteCmdLine(command);
            else bConsole.StartInteractive();
        }
    }
}