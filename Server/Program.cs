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
        private static string _UnixSocketPath = null;

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
                if (_UnixSocketPath != null)
                    try { File.Delete(_UnixSocketPath); }
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

            logger.LogLine("Loading booru...");
            ServerBooru booru = new ServerBooru(booruPath);

            Server server = null;
            SocketListener sockListener = null;
            MailNotificator mn = null;
            try
            {
                logger.LogLine("Binding socket...");
                var parsedSocket = SocketParser.Parse(config.Socket);
                // if (File.Exists(unixSocketPath))
                // {
                //     logger.LogLine("Socket exists, removing it...");
                //     File.Delete(unixSocketPath);
                // }
                parsedSocket.Socket.Bind(parsedSocket.EndPoint);
                if (parsedSocket.UnixSocketPath != null)
                {
                    SyscallEx.chmod(parsedSocket.UnixSocketPath,
                        FilePermissions.S_IFSOCK |
                        FilePermissions.S_IRUSR |
                        FilePermissions.S_IWUSR |
                        FilePermissions.S_IRGRP |
                        FilePermissions.S_IWGRP);
                    SyscallEx.chown(parsedSocket.UnixSocketPath, config.User);
                    _UnixSocketPath = parsedSocket.UnixSocketPath;
                }

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
                if (parsedSocket.Socket != null)
                {
                    sockListener = new SocketListener(parsedSocket.Socket);
                    sockListener.SocketAccepted += socket =>
                        {
                            logger.LogLine("Client connected");
                            server.AddAcceptedSocket(socket);
                        };
                    sockListener.Start();
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
                sockListener.Dispose();
            }

            logger.LogLine("Closing booru...");
            booru.Dispose();
        }
    }
}
