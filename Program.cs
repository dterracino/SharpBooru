using System;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using TA.SharpBooru.Client;
using TA.SharpBooru.Server;
using TA.SharpBooru.Client.GUI;
using TA.SharpBooru.Client.CLI;
using TA.SharpBooru.Client.WebServer;
using TA.SharpBooru.Client.WebServer.VFS;
using CommandLine;

namespace TA.SharpBooru
{
    public class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            Console.Title = "SharpBooru";
            Console.Write(Properties.Resources.ascii_banner);
            Console.WriteLine();
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
                        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, options.Port);
                        wrapper.StartServer(options.Location, options.Username, localEndPoint);
                    }
                    else if (options.Mode == Options.RunMode.Standalone)
                    {
                        Console.Title = "SharpBooru Standalone";
                        ServerWrapper wrapper = new ServerWrapper(sLogger);
                        string location = options.Location ?? Environment.CurrentDirectory;
                        wrapper.StartServer(location, null, new IPEndPoint(IPAddress.Loopback, 0), false);
                        IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Loopback, wrapper.BooruServer.Port);
                        using (BooruClient booru = ConnectBooru(serverEndPoint, options.Username, options.Password))
                            RunClientGUI(booru);
                        wrapper.Cancel(null);
                    }
                    else using (BooruClient booru = ConnectBooru(options.Server, options.Username, options.Password))
                            switch (options.Mode)
                            {
                                case Options.RunMode.GUI: RunClientGUI(booru); break;
                                case Options.RunMode.CLI: RunClientCLI(booru, options.Command); break;
                                case Options.RunMode.WebServer: RunClientWebserver(booru, options.Port, sLogger); break;
                            }
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

        private static BooruClient ConnectBooru(IPEndPoint endPoint, string username = null, string password = null)
        {
            BooruClient client = new BooruClient();
            client.Connect(endPoint);
            if (username != null && password != null)
                client.Login(username, password);
            return client;
        }

        private static void RunClientWebserver(BooruClient booru, ushort port, Logger logger)
        {
            Console.Title = "SharpBooru Webserver";

            port = port < 1 ? (ushort)80 : port;
            BooruWebServer server = new BooruWebServer(booru, logger, string.Format("http://*:{0}/", port), false);

            //Server.RootDirectory.Add(new VFSLoginLogoutFile("login_logout"));
            //Server.RootDirectory.Add(new VFSAdminPanelFile("admin"));
            server.RootDirectory.Add(new VFSBooruImageFile("image", true));
            server.RootDirectory.Add(new VFSBooruImageFile("thumb", false));
            server.RootDirectory.Add(new VFSBooruPostFile("post", "Post #{0}"));
            server.RootDirectory.Add(new VFSByteFile("favicon.ico", "image/x-icon", Properties.Resources.favicon_ico));
            server.RootDirectory.Add(new VFSBooruSearchFile("index", "Search"));
            //Server.RootDirectory.Add(new VFSBooruInfoFile("info"));
            server.RootDirectory.Add(new VFSDelegateFile("style", "text/css", c => c.OutWriter.Write(WebserverHelper.GetStyle(c)), false, null));
            server.RootDirectory.Add(new VFSStringFile("robots.txt", "text/plain", "User-agent: *\r\nDisallow: /", false, null));
            //Server.RootDirectory.Add(new VFSBooruUploadFile("upload"));

            server.Start();

            logger.LogLine("Changing UID to user 'nobody'...");
            try { ServerHelper.SetUID("nobody"); }
            catch (Exception ex) { logger.LogException("SetUID", ex); }

            Thread.Sleep(Timeout.Infinite);
        }

        private static void RunClientGUI(BooruClient booru)
        {
            //TODO Show connect dialog when connection string not given
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (MainForm mForm = new MainForm(booru))
            {
                GUIHelper.HideConsoleWindow();
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