using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
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

        private static ClientBooru ConnectBooru(Options options) { return new ClientBooru(options.Server, options.Username ?? "guest", options.Password ?? "guest"); }

        private static int RunClientGUI(Options options)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //TODO Show connect dialog
            using (ClientBooru booru = ConnectBooru(options))
            using (MainForm mForm = new MainForm(booru))
            {
                GUIHelper.HideConsoleWindow();
                mForm.ShowDialog();
            }
            return 0;
        }

        private static int RunClientCLI(Options options)
        {
            using (ClientBooru booru = ConnectBooru(options))
            {
                booru.Connect();
                BooruConsole bConsole = new BooruConsole(booru);
                if (!string.IsNullOrWhiteSpace(options.Command))
                    bConsole.ExecuteCmdLine(options.Command);
                else bConsole.StartInteractive();
            }
            return 0;
        }
    }
}