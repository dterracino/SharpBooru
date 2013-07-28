using System;
using System.Threading;
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

            if (args.Length != 1)
                throw new ArgumentException("Server needs one argument (Booru path)");

            sLogger.LogLine("Loading booru from disk...");
            Booru sBooru = Booru.Exists(args[0]) ?
                Booru.ReadFromDisk(args[0])
                : new Booru() { Folder = args[0] };
            sLogger.LogLine("Finished loading booru with {0} posts", sBooru.Posts.Count);

            BooruServer server = new BooruServer(sBooru, sLogger, sCertificate);

            EventWaitHandle waitEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
            Action cancelHandler = () => //TODO Make threadsafe
                {
                    sLogger.LogLine("Stopping server and waiting for clients to finish...");
                    server.Stop();
                    server.WaitForClients(10);
                    sLogger.LogLine("Saving booru to disk...");
                    server.Booru.SaveToDisk();
                    waitEvent.Set();
                };

            Console.CancelKeyPress += (sender, e) => cancelHandler();
            if (Helper.IsPOSIX())
            {
                SetupSignal(Signum.SIGUSR1, server.Booru.SaveToDisk);
                SetupSignal(Signum.SIGTERM, cancelHandler);
            }

            server.Start(); //TODO SetUID
            waitEvent.WaitOne(); //Wait for cancelHandler to finish
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