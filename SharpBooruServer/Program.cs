using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
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
                    (new Program(options, sLogger)).Run();
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

        public Program(Options Options, Logger Logger)
        {
            _Options = Options;
            _Logger = Logger;
        }

        private Options _Options;
        private Logger _Logger;
        private Booru _Booru;
        private Thread _AutoSaveThread;
        private bool _AutoSaveRunning = true;
        private ServerBroadcaster _ServerBroadcaster;
        private BooruServer _BooruServer;

        public void Run()
        {
            if (_Options.PIDFile != null)
            {
                if (!File.Exists(_Options.PIDFile))
                {
                    _Logger.LogLine("Writing PID file...");
                    Process currentProcess = Process.GetCurrentProcess();
                    File.WriteAllText(_Options.PIDFile, currentProcess.Id.ToString(), Encoding.ASCII); //TODO Use FileStream
                    if (_Options.User != null)
                    {
                        _Logger.LogLine("Changing PID file ownership since we use SetUID later...");
                        ServerHelper.ChOwn(_Options.PIDFile, _Options.User);
                    }
                }
                else throw new Exception("PIDFile exists, make sure no 2nd instance is running and delete the PID file");
            }

            _Logger.LogLine("Loading certificate...");
            string certificateFile = _Options.Certificate ?? Path.Combine(_Options.Location, "cert.pfx");
            X509Certificate2 sCertificate = new X509Certificate2(certificateFile, _Options.CertificatePassword);
            _Logger.LogLine("Loading booru from disk...");
            _Booru = Booru.ReadFromDisk(_Options.Location);
            _Logger.LogLine("Finished loading booru - {0} posts / {1} tags", _Booru.Posts.Count, _Booru.Tags.Count);

            _Logger.LogLine("Starting AutoSave thread...");
            _AutoSaveThread = new Thread(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(5 * 60 * 1000);
                        if (_AutoSaveRunning)
                        {
                            _Booru.SaveToDisk();
                            _Logger.LogLine("AutoSave: Booru saved to disk");
                        }
                        else break;
                    }
                }) { IsBackground = true };
            _AutoSaveThread.Start();

            _BooruServer = new BooruServer(_Booru, _Logger, sCertificate, _Options.Port);

            _Logger.LogLine("Starting ServerBroadcaster...");
            _ServerBroadcaster = new ServerBroadcaster(_Booru, _Options.Port);
            _ServerBroadcaster.Start();

            EventWaitHandle waitEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
            _Logger.LogLine("Registering CtrlC handler...");
            Console.CancelKeyPress += (sender, e) => Cancel(waitEvent);
            if (Helper.IsUnix())
            {
                _Logger.LogLine("Registering SIGTERM handler...");
                ServerHelper.SetupSignal(Signum.SIGTERM, () => Cancel(waitEvent));
            }

            _Logger.LogLine("Starting server...");
            _BooruServer.Start();

            if (_Options.User != null)
                try { ServerHelper.SetUID(_Options.User); }
                catch (Exception ex) { _Logger.LogException("SetUID", ex); }

            _Logger.LogLine("Server running.");
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
                if (_Options.PIDFile != null)
                {
                    _Logger.LogLine("Removing PID file...");
                    try { File.Delete(_Options.PIDFile); }
                    catch (Exception ex) { _Logger.LogException("DeletePIDFile", ex); }
                }
                _CancelRunned = true;
            }
        }
    }
}
