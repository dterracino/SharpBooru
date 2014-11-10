using System;
using System.IO;
using System.Net.Mail;
using System.Net.Sockets;
using Mono.Unix;
using Mono.Unix.Native;

namespace TA.SharpBooru.Server
{
    public class Program
    {
        private static bool _SocketCreated = false;

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
                if (_SocketCreated)
                    try
                    {
                        string unixSocketPath = Path.Combine(booruPath, "socket.sock");
                        File.Delete(unixSocketPath);
                    }
                    catch { }
            }
        }

        private static void MainStage2(Logger logger, string booruPath)
        {
            Console.Write("\x1b]0;SharpBooru Server\x07");
            // Console.Title = "SharpBooru Server";
            Console.TreatControlCAsInput = true;

            if (Environment.OSVersion.Platform != PlatformID.Unix)
                throw new PlatformNotSupportedException("Only Linux is supported");

            if (Type.GetType("Mono.Runtime") == null)
                throw new PlatformNotSupportedException("Only Mono is supported");

            logger.LogLine("Loading configuration...");
            Config config = new Config("config.xml");

            string unixSocketPath = config.Socket;
            UnixEndPoint unixEndPoint = new UnixEndPoint(unixSocketPath);

            logger.LogLine("Loading booru...");
            ServerBooru booru = new ServerBooru(booruPath);

            Server server = null;
            SocketListener unixListener = null;
            MailNotificator mn = null;
            try
            {
                Socket unixSocket = null;
                logger.LogLine("Binding UNIX socket...");
                if (File.Exists(unixSocketPath))
                {
                    logger.LogLine("Socket exists, removing it...");
                    File.Delete(unixSocketPath);
                }
                unixSocket = new Socket(AddressFamily.Unix, SocketType.Stream, 0);
                unixSocket.Bind(unixEndPoint);
                _SocketCreated = true;
                SyscallEx.chmod(unixSocketPath,
                    FilePermissions.S_IFSOCK |
                    FilePermissions.S_IRUSR |
                    FilePermissions.S_IWUSR |
                    FilePermissions.S_IRGRP |
                    FilePermissions.S_IWGRP);
                SyscallEx.chown(unixSocketPath, config.User);

                logger.LogLine("Changing UID to {0}...", config.User);
                SyscallEx.setuid(config.User);

                logger.LogLine("Starting server...");
                mn = new MailNotificator(logger,
                    config.MailNotificatorServer,
                    config.MailNotificatorPort,
                    config.MailNotificatorUsername,
                    config.MailNotificatorPassword,
                    config.MailNotificatorSender,
                    config.MailNotificatorReceiver);
                server = new Server(booru, logger, mn, 2);
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
