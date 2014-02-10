using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using TA.SharpBooru.Client;
using TA.SharpBooru.Client.GUI;
using TA.SharpBooru.Client.CLI;
using CommandLine;

namespace TA.SharpBooru
{
    public class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            Console.Title = "SharpBooru";
            Logger sLogger = new Logger(Console.Out);
            Options options = new Options();
            try
            {
                if (Parser.Default.ParseArguments(args, options))
                {
                    int returnCode = 0;
                    switch (options.Mode)
                    {
                        case Options.RunMode.GUI: returnCode = RunClientGUI(options); break;
                        case Options.RunMode.CLI: returnCode = RunClientCLI(options); break;
                        case Options.RunMode.Server: Server.Program.subMain(options, sLogger); break;
                        case Options.RunMode.WebServer: throw new NotImplementedException();
                    }
                    Helper.CleanTempFolder();
                    return returnCode;
                }
            }
            catch (Exception ex) { sLogger.LogException("SharpBooru", ex); }
            return 1;
        }

        private static BooruClient ConnectBooru(Options options) 
        {
            BooruClient client = new BooruClient();
            client.Connect(options.Server);
            if (options.Username != null && options.Password != null)
                client.Login(options.Username, options.Password);
            return client;
        }

        private static int RunClientGUI(Options options)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //TODO Show connect dialog
            using (BooruClient booru = ConnectBooru(options))
            using (MainForm mForm = new MainForm(booru))
            {
                GUIHelper.HideConsoleWindow();
                mForm.ShowDialog();
            }
            return 0;
        }

        private static int RunClientCLI(Options options)
        {
            using (BooruClient booru = ConnectBooru(options))
            {
                BooruConsole bConsole = new BooruConsole(booru);
                if (!string.IsNullOrWhiteSpace(options.Command))
                    bConsole.ExecuteCmdLine(options.Command);
                else bConsole.StartInteractive();
            }
            return 0;
        }
    }
}