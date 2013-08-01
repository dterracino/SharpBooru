using System;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Mono.Unix;
using Mono.Unix.Native;

namespace TA.SharpBooru.Server
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Console.Title = "SharpBooru Server";
            Logger sLogger = new Logger(Console.Out);
            try
            {
                (new Program()).Run(args, sLogger);
                return 0;
            }
            catch (Exception ex)
            {
                sLogger.LogException(ex);
                return 1;
            }
        }

        public void Run(string[] args, Logger sLogger)
        {
            X509Certificate sCertificate = new X509Certificate("ServerCertificate.pfx", "sharpbooru");

            if (args.Length != 2)
                throw new ArgumentException("Server needs two argument, Port and Booru path");

            sLogger.LogLine("Loading booru from disk...");
            Booru sBooru = Booru.ReadFromDisk(args[1]);
            sLogger.LogLine("Finished loading booru with {0} posts and {1} tags", sBooru.Posts.Count, sBooru.Tags.Count);

            ushort serverPort = Convert.ToUInt16(args[0]);
            BooruServer server = new BooruServer(sBooru, sLogger, sCertificate, serverPort);

            EventWaitHandle waitEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
            Console.CancelKeyPress += (sender, e) => Cancel(server, sLogger, waitEvent);
            if (Helper.IsPOSIX())
            {
                SetupSignal(Signum.SIGUSR1, server.Booru.SaveToDisk);
                SetupSignal(Signum.SIGTERM, () => Cancel(server, sLogger, waitEvent));
            }

            server.Start(); //TODO SetUID
            waitEvent.WaitOne(); //Wait for Cancel to finish
        }

        private bool _CancelRunned = false;
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Cancel(BooruServer Server, Logger Logger, EventWaitHandle WaitEvent)
        {
            if (!_CancelRunned)
            {
                Logger.LogLine("Stopping server and waiting for clients to finish...");
                Server.Stop();
                Server.WaitForClients(10);
                Logger.LogLine("Saving booru to disk...");
                Server.Booru.SaveToDisk();
                WaitEvent.Set();
                _CancelRunned = true;
            }
        }

        public static void SetupSignal(Signum Signal, Action SignalAction)
        {
            Thread signalThread = new Thread(() =>
                {
                    using (UnixSignal uSignal = new UnixSignal(Signal))
                        uSignal.WaitOne(Timeout.Infinite);
                    SignalAction();
                }) { IsBackground = true };
            signalThread.Start();
        }

        public static bool SetUID(string UserName)
        {
            if (!string.IsNullOrWhiteSpace(UserName))
            {
                Passwd passwordStruct = Syscall.getpwnam(UserName);
                if (passwordStruct != null)
                    return Syscall.setuid(passwordStruct.pw_uid) == 0;
            }
            return false;
        }
    }
}