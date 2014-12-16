using System;
using System.IO;
using System.Net.Mail;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Mono.Unix;
using Mono.Unix.Native;

namespace TA.SharpBooru.Server
{
    public class Program
    {
        private static List<string> _UnixSocketPaths = new List<string>(1);

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
                foreach (string path in _UnixSocketPaths)
                    try { File.Delete(path); }
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

            X509Certificate2 cert = null;
            if (config.CertificateNeeded)
            {
                logger.LogLine("Loading certificate...");
                cert = new X509Certificate2(config.Certificate);
            }

            logger.LogLine("Loading booru...");
            ServerBooru booru = new ServerBooru(booruPath);

            Server server = null;
            SocketListener[] sockListeners = null;
            MailNotificator mn = null;
            try
            {
                logger.LogLine("Binding sockets...");
                // if (File.Exists(unixSocketPath))
                // {
                //     logger.LogLine("Socket exists, removing it...");
                //     File.Delete(unixSocketPath);
                // }
                Socket[] sockets = new Socket[config.SocketConfigs.Count];
                sockListeners = new SocketListener[sockets.Length];
                for (byte i = 0; i < sockets.Length; i++)
                {
                    var sockConf = config.SocketConfigs[i];
                    sockets[i] = sockConf.Socket;
                    sockets[i].Bind(sockConf.EndPoint);
                    if (sockConf.UnixSocketPath != null)
                    {
                        SyscallEx.chmod(sockConf.UnixSocketPath, sockConf.UnixSocketPerms);
                        SyscallEx.chown(sockConf.UnixSocketPath, config.User);
                        _UnixSocketPaths.Add(sockConf.UnixSocketPath);
                    }
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
                for (int i = 0; i < sockets.Length; i++)
                {
                    bool useTLS = config.SocketConfigs[i].UseTLS;
                    sockListeners[i].SocketAccepted += socket =>
                        {
                            logger.LogLine("Client connected");
                            NetworkStream ns = new NetworkStream(socket, true);
                            server.AddConnectedClient(ns, useTLS ? cert : null);
                        };
                    sockListeners[i].Start();
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
                foreach (var listener in sockListeners)
                    listener.Dispose();
            }

            logger.LogLine("Closing booru...");
            booru.Dispose();
        }
    }
}
