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
            Server.RootDirectory.Add(new VFSLoginLogoutFile("login_logout"));
            Server.RootDirectory.Add(new VFSAdminPanelFile("admin"));
            Server.RootDirectory.Add(new VFSBooruImageFile("image", true));
            Server.RootDirectory.Add(new VFSBooruImageFile("thumb", false));
            Server.RootDirectory.Add(new VFSBooruPostFile("post", "Post #{0}"));
            Server.RootDirectory.Add(new VFSByteFile("favicon.ico", "image/x-icon", Properties.Resources.favicon_ico));
            Server.RootDirectory.Add(new VFSBooruSearchFile("index", "Search"));
            Server.RootDirectory.Add(new VFSBooruInfoFile("info"));
            Server.RootDirectory.Add(new VFSDelegateFile("style", "text/css", c => c.OutWriter.Write(ServerHelper.GetStyle(c)), false, null));
            Server.RootDirectory.Add(new VFSStringFile("robots.txt", "text/plain", "User-agent: *\r\nDisallow: /", false, null));
            Server.RootDirectory.Add(new VFSBooruUploadFile("upload"));
        }

        private ServerConfig LoadConfig(string[] args)
        {
            string path = args.Length == 1 ? args[0] : "config.json";
            if (!File.Exists(path))
            {
                ServerConfig config = new ServerConfig();
                config.SaveToFile(path);
                return config;
            }
            else return ServerConfig.LoadFromFile(path);
        }

        public int Run(string[] args)
        {
            Logger logger = new Logger(Console.Out, Helper.IsMono());
            if (!(Helper.IsMono() && Helper.IsPOSIX()))
            {
                logger.LogLine("You are not running the server inside the Mono runtime or a POSIX system");
                logger.LogLine("If you are not using a POSIX system and Mono, some features will not work:");
                logger.LogLine("  * SetUID (important security feature)");
                logger.LogLine("  * Colored console output");
                logger.LogLine("  * Gracefull shutdown via SIGTERM");
                logger.LogLine("Please run the server inside of Mono for best security and performance");
                logger.LogLine("The server will still be started, but please think about the message above");
                logger.LogLine();
            }
            ServerConfig config = LoadConfig(args);
            logger.LogBegin("Loading database wrapper");
            SQLWrapper wrapper = new MySQLWrapper(config.ConnectionInfo as MySQLWrapper.DBConnectionInfo);
            logger.LogOK();
            logger.LogBegin("Testing database connection");
            Exception exception = wrapper.TestConnection();
            if (exception != null)
                return logger.LogFAILAndThrow(exception);
            else logger.LogOK();
            logger.LogBegin("Checking database compatibility");
            SQLWrapper.DatabaseCompatibility compatibility = wrapper.GetDatabaseCompatibility();
            if (compatibility != SQLWrapper.DatabaseCompatibility.Compatible)
                return logger.LogFAILAndThrow(new Exception(string.Format("Database not compatible. Please update the {0}", compatibility == SQLWrapper.DatabaseCompatibility.UpdateDatabase ? "database" : "server")));
            else
            {
                logger.LogOK();
                Booru booru = new Booru(wrapper);
                logger.LogBegin("Checking if booru is a server booru");
                if (!booru.GetProperty<bool>(Booru.SystemProperty.IsServerBooru))
                    return logger.LogFAILAndThrow(new Exception("Booru is no server booru"));
                else
                {
                    logger.LogOK();
                    BooruServer server = new BooruServer(booru, logger, config.ListenerPrefix, false);
                    InitVFS(server, booru);
                    server.Start();
                    if (!ServerHelper.SetUID(config.User))
                    {
                        server.Stop();
                        throw new System.Security.SecurityException("Could not set UID - server stopped");
                    }
                    else if (Helper.IsMono())
                    {
                        UnixSignal signal = new UnixSignal(Signum.SIGTERM);
                        signal.WaitOne(-1);
                        logger.LogLine("[c3]Received SIGTERM[cr] - [c7]stopping server gracefully...");
                        server.Stop();
                    }
                    else Thread.Sleep(Timeout.Infinite);
                    return 0;
                }
            }
        }
    }
}