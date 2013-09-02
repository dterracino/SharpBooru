using System;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Mono.Unix.Native;
using CommandLine;

namespace TA.SharpBooru.Server
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Console.Title = "SharpBooru Server";
            Logger sLogger = new Logger(Console.Out);
            Options options = new Options();
            try
            {
                if (Parser.Default.ParseArguments(args, options))
                {
                    (new Program(sLogger)).Run(options);
                    return 0;
                }
                else return 1;
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
        private ServerBroadcaster _ServerBroadcaster;
        private BooruServer _BooruServer;

        public void Run(Options options)
        {
            X509Certificate sCertificate = new X509Certificate("ServerCertificate.pfx", "sharpbooru");

            _Logger.LogLine("Loading booru from disk...");
            _Booru = Booru.ReadFromDisk(options.Location);
            _Logger.LogLine("Finished loading booru - {0} posts / {1} tags", _Booru.Posts.Count, _Booru.Tags.Count);

            _AutoSaveThread = new Thread(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(5 * 60 * 1000);
                        if (_AutoSaveRunning)
                        {
                            _Booru.SaveToDisk();
                            _Logger.LogLine("AutoSave: Booru saved");
                        }
                        else break;
                    }
                }) { IsBackground = true };
            _AutoSaveThread.Start();

            _BooruServer = new BooruServer(_Booru, _Logger, sCertificate, options.Port);

            _ServerBroadcaster = new ServerBroadcaster(_Booru, options.Port);
            _ServerBroadcaster.Start();

            EventWaitHandle waitEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
            Console.CancelKeyPress += (sender, e) => Cancel(waitEvent);
            if (Helper.IsUnix())
            {
                ServerHelper.SetupSignal(Signum.SIGUSR1, _Booru.SaveToDisk);
                ServerHelper.SetupSignal(Signum.SIGTERM, () => Cancel(waitEvent));
            }

            _BooruServer.Start();

            try { ServerHelper.SetUID(options.User); }
            catch (Exception ex) { _Logger.LogException("SetUID", ex); }

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
                _ServerBroadcaster.Stop();
                _BooruServer.Stop();
                //_BooruServer.WaitForClients(10);
                _Logger.LogLine("Saving booru to disk...");
                _Booru.SaveToDisk();
                WaitEvent.Set();
                _CancelRunned = true;
            }
        }
    }
}