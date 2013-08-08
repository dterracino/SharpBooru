using System;
using System.IO;
using System.Net;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;
using TA.SharpBooru.Client;
using TA.SharpBooru.Client.WebServer.VFS;

namespace TA.SharpBooru.Client.WebServer
{
    public class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            Program program = new Program();
            try { return program.Run(args); }
            catch (Exception ex)
            {
                Console.WriteLine("FATAL ERROR: {0}", ex.Message);
                return 1;
            }
        }

        private void InitVFS(BooruServer Server, Booru Booru)
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

        public int Run(string[] args)
        {
            Logger _Logger = new Logger(Console.Out);
            Booru _Booru = new Booru("localhost", 2401, "guest", "guest");
            BooruServer _Server = new BooruServer(_Booru, _Logger, "http://*:8080/", false);

            InitVFS(_Server, _Booru);
            _Server.Start();

            Thread.Sleep(Timeout.Infinite);
            return 0;
        }
    }
}