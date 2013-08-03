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
                (new Program(sLogger)).Run(args);
                return 0;
            }
            catch (Exception ex)
            {
                sLogger.LogException("Main", ex);
                return 1;
            }
        }

        public Program(Logger Logger) { _Logger = Logger; }

        private Logger _Logger;
        private Booru _Booru;
        private Thread _AutoSaveThread;
        private bool _AutoSaveRunning = true;
        private BooruServer _BooruServer;

        public void Run(string[] args)
        {
            X509Certificate sCertificate = new X509Certificate("ServerCertificate.pfx", "sharpbooru");

            if (args.Length != 2)
                throw new ArgumentException("Server needs two argument, Port and Booru path");

            _Logger.LogLine("Loading booru from disk...");
            _Booru = Booru.ReadFromDisk(args[1]);
            _Logger.LogLine("Finished loading booru - {0} posts / {1} tags", _Booru.Posts.Count, _Booru.Tags.Count);

            _AutoSaveThread = new Thread(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(5 * 60 * 1000);
                        if (_AutoSaveRunning)
                            _Booru.SaveToDisk();
                        else break;
                    }
                }) { IsBackground = true };
            _AutoSaveThread.Start();

            ushort serverPort = Convert.ToUInt16(args[0]);
            _BooruServer = new BooruServer(_Booru, _Logger, sCertificate, serverPort);

            EventWaitHandle waitEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
            Console.CancelKeyPress += (sender, e) => Cancel(waitEvent);
            if (Helper.IsUnix())
            {
                ServerHelper.SetupSignal(Signum.SIGUSR1, _Booru.SaveToDisk);
                ServerHelper.SetupSignal(Signum.SIGTERM, () => Cancel(waitEvent));
            }

            _BooruServer.Start("nobody");
            waitEvent.WaitOne(); //Wait for Cancel to finish
        }

        private bool _CancelRunned = false;
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Cancel(EventWaitHandle WaitEvent)
        {
            if (!_CancelRunned)
            {
                _Logger.LogLine("Stopping server and waiting for clients to finish...");
                _AutoSaveRunning = false;
                _BooruServer.Stop();
                _BooruServer.WaitForClients(10);
                _Logger.LogLine("Saving booru to disk...");
                _Booru.SaveToDisk();
                WaitEvent.Set();
                _CancelRunned = true;
            }
        }
    }
}