using System;
using System.Threading;
using CommandLine;
using TA.SharpBooru.Client.WebServer.VFS;

namespace TA.SharpBooru.Client.WebServer
{
    public class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            Console.Title = "SharpBooru WebServer";
            Logger sLogger = new Logger(Console.Out);
            Options options = new Options();
            try
            {
                if (Parser.Default.ParseArguments(args, options))
                {
                    (new Program()).Run(options, sLogger);
                    return 0;
                }
                else return 1;
            }
            catch (Exception ex) { sLogger.LogException("WebServer", ex); }
            return 1;
        }

        private void InitVFS(BooruWebServer Server, ClientBooru Booru)
        {
            //Server.RootDirectory.Add(new VFSLoginLogoutFile("login_logout"));
            //Server.RootDirectory.Add(new VFSAdminPanelFile("admin"));
            Server.RootDirectory.Add(new VFSBooruImageFile("image", true));
            Server.RootDirectory.Add(new VFSBooruImageFile("thumb", false));
            Server.RootDirectory.Add(new VFSBooruPostFile("post", "Post #{0}"));
            Server.RootDirectory.Add(new VFSByteFile("favicon.ico", "image/x-icon", Properties.Resources.favicon_ico));
            Server.RootDirectory.Add(new VFSBooruSearchFile("index", "Search"));
            //Server.RootDirectory.Add(new VFSBooruInfoFile("info"));
            Server.RootDirectory.Add(new VFSDelegateFile("style", "text/css", c => c.OutWriter.Write(ServerHelper.GetStyle(c)), false, null));
            Server.RootDirectory.Add(new VFSStringFile("robots.txt", "text/plain", "User-agent: *\r\nDisallow: /", false, null));
            //Server.RootDirectory.Add(new VFSBooruUploadFile("upload"));
        }

        public int Run(Options options, Logger logger)
        {
            ClientBooru booru = new ClientBooru(Helper.GetIPEndPointFromString(options.Server), options.Username, options.Password);
            BooruWebServer server = new BooruWebServer(booru, logger, string.Format("http://*:{0}/", options.Port), false);

            InitVFS(server, booru);
            server.Start();

            Thread.Sleep(Timeout.Infinite);
            return 0;
        }
    }
}