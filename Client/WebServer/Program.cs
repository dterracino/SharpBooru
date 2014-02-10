using System;
using System.Threading;
using CommandLine;
using TA.SharpBooru.Client.WebServer.VFS;

namespace TA.SharpBooru.Client.WebServer
{
    public class Program
    {
        private void InitVFS(BooruWebServer Server, BooruClient Booru)
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

        public int Run(BooruClient booru, Options options, Logger logger)
        {
            ushort port = options.Port < 1 ? (ushort)80 : options.Port;
            BooruWebServer server = new BooruWebServer(booru, logger, string.Format("http://*:{0}/", port), false);

            InitVFS(server, booru);
            server.Start();

            try { ServerHelper.SetUID("nobody"); }
            catch { Console.WriteLine("SetUID FAILED"); }

            Thread.Sleep(Timeout.Infinite);
            return 0;
        }
    }
}