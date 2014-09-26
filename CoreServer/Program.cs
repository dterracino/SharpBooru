using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Net.Sockets;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;

namespace TA.SharpBooru
{
    public class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            Logger logger = new Logger(Console.Out);
            string booruPath = Environment.CurrentDirectory;
            try
            {
                MainStage2(logger, booruPath);
                return 0;
            }
            catch (Exception ex)
            {
                logger.LogException("MainStage2", ex);
                return 1;
            }
            finally
            {
                try { Helper.CleanTempFolder(); }
                catch { }
                try
                {
                    string unixSocketPath = Path.Combine(booruPath, "socket.sock");
                    SyscallEx.unlink(unixSocketPath);
                }
                catch { }
            }
        }

        private static void MainStage2(Logger logger, string booruPath)
        {
            Console.Title = "SharpBooru Server";
            Console.TreatControlCAsInput = true;

            if (Environment.OSVersion.Platform != PlatformID.Unix)
                throw new PlatformNotSupportedException("Only Linux is supported");

            if (Type.GetType("Mono.Runtime") == null)
                throw new PlatformNotSupportedException("Only Mono is supported");

            string unixSocketPath = Path.Combine(booruPath, "socket.sock");
            UnixEndPoint unixEndPoint = new UnixEndPoint(unixSocketPath);
            string user = "booru";

            logger.LogLine("Loading booru...");
            ServerBooru booru = new ServerBooru(booruPath);

            Server server = null;
            SocketListener unixListener = null;
            try
            {
                Socket unixSocket = null;
                logger.LogLine("Binding UNIX socket...");
                unixSocket = new Socket(AddressFamily.Unix, SocketType.Stream, 0);
                unixSocket.Bind(unixEndPoint);
                SyscallEx.chmod(unixSocketPath,
                    FilePermissions.S_IFSOCK |
                    FilePermissions.S_IRUSR |
                    FilePermissions.S_IWUSR |
                    FilePermissions.S_IRGRP |
                    FilePermissions.S_IWGRP);
                SyscallEx.chown(unixSocketPath, user);

                logger.LogLine("Changing UID to {0}...", user);
                SyscallEx.setuid(user);

                logger.LogLine("Starting server...");
                server = new Server(booru, logger);
                if (unixSocket != null)
                {
                    unixListener = new SocketListener(unixSocket);
                    unixListener.SocketAccepted += socket =>
                        {
                            logger.LogLine("Client connected");
                            server.AddAcceptedSocket(socket);
                        };
                    unixListener.Start();
                }

                // Ctrl+C is not supported due to heavy crashes of Mono
                using (UnixSignal sigtermSignal = new UnixSignal(Signum.SIGTERM))
                {
                    logger.LogLine("Startup finished, waiting for SIGTERM...");
                    sigtermSignal.WaitOne();
                }
            }
            catch (Exception ex) { logger.LogException("MainStage3", ex); }
            finally
            {
                logger.LogLine("Stopping server and closing sockets...");
                server.Dispose();
                unixListener.Dispose();
            }

            logger.LogLine("Closing booru...");
            booru.Dispose();
        }
    }
}
