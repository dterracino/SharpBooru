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
            try
            {
                MainStage2(logger);
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
            }
        }

        private static void MainStage2(Logger logger)
        {
            Console.Title = "SharpBooru Server";

            if (Environment.OSVersion.Platform != PlatformID.Unix)
                throw new PlatformNotSupportedException("Only Linux is supported");

            if (Type.GetType("Mono.Runtime") == null)
                throw new PlatformNotSupportedException("Only Mono is supported");

            string dir = Environment.CurrentDirectory;

            logger.LogLine("Reading configuration...");
            XmlDocument config = new XmlDocument();
            config.Load(Path.Combine(dir, "config.xml"));
            XmlNode booruConfigNode = config.SelectSingleNode("/BooruConfig");

            string user = booruConfigNode.SelectSingleNode("User").InnerText;

            string unixSocketPath = null;
            UnixEndPoint unixEndPoint = null;
            XmlNode unixSocketNode = booruConfigNode.SelectSingleNode("UnixSocket");
            if (Convert.ToBoolean(unixSocketNode.SelectSingleNode("Enabled").InnerText))
            {
                unixSocketPath = unixSocketNode.SelectSingleNode("Path").InnerText;
                unixEndPoint = new UnixEndPoint(unixSocketPath);
            }

            IPEndPoint tcpEndPoint = null;
            XmlNode tcpSocketNode = booruConfigNode.SelectSingleNode("TcpSocket");
            if (Convert.ToBoolean(tcpSocketNode.SelectSingleNode("Enabled").InnerText))
                tcpEndPoint = new IPEndPoint(
                    IPAddress.Parse(tcpSocketNode.SelectSingleNode("Address").InnerText),
                    Convert.ToUInt16(tcpSocketNode.SelectSingleNode("Port").InnerText));

            if (unixEndPoint == null && tcpEndPoint == null)
                throw new NotSupportedException("No sockets enabled");

            logger.LogLine("Loading booru...");
            ServerBooru booru = new ServerBooru(dir);

            Server server = null;
            SocketListener unixListener = null, tcpListener = null;
            try
            {
                Socket unixSocket = null;
                if (unixEndPoint != null)
                {
                    logger.LogLine("Binding UNIX socket...");
                    unixSocket = new Socket(AddressFamily.Unix, SocketType.Stream, 0);
                    unixSocket.Bind(unixEndPoint);
                    SyscallEx.chown(unixSocketPath, user);
                    SyscallEx.chmod(unixSocketPath,
                        FilePermissions.S_IFSOCK |
                        FilePermissions.S_IRUSR |
                        FilePermissions.S_IWUSR |
                        FilePermissions.S_IRGRP |
                        FilePermissions.S_IWGRP);
                }
                Socket tcpSocket = null;
                if (tcpEndPoint != null)
                {
                    logger.LogLine("Binding TCP socket...");
                    tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    tcpSocket.Bind(tcpEndPoint);
                }

                logger.LogLine("Changing UID to {0}...", user);
                SyscallEx.setuid(user);

                logger.LogLine("Starting server...");
                server = new Server();
                if (unixSocket != null)
                {
                    unixListener = new SocketListener(unixSocket);
                    unixListener.SocketAccepted += socket => server.AddAcceptedSocket(false);
                    unixListener.Start();
                }
                if (tcpSocket != null)
                {
                    tcpListener = new SocketListener(tcpSocket);
                    tcpListener.SocketAccepted += socket => server.AddAcceptedSocket(true);
                    tcpListener.Start();
                }

                logger.LogLine("Registering exit handlers...");
                WaitHandle[] waitHandles = new WaitHandle[2];
                waitHandles[0] = new UnixSignal(Signum.SIGTERM);
                waitHandles[1] = new ManualResetEvent(false);
                Console.CancelKeyPress += (sender, e) => ((ManualResetEvent)waitHandles[1]).Set();

                logger.LogLine("Startup finished, waiting for exit event...");
                WaitHandle.WaitAny(waitHandles);
                waitHandles[0].Dispose();
                waitHandles[1].Dispose();
            }
            catch (Exception ex) { logger.LogException("MainStage3", ex); }
            finally
            {
                logger.LogLine("Stopping server and closing sockets...");
                server.Dispose();
                if (unixListener != null)
                {
                    unixListener.Dispose();
                    SyscallEx.unlink(unixSocketPath);
                }
                if (tcpListener != null)
                    tcpListener.Dispose();
            }

            logger.LogLine("Closing booru...");
            booru.Dispose();
        }
    }
}
