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
        private Thread _BroadcastListenerThread;
        private bool _BroadcastListenerThreadRunning = true;
        private BooruServer _BooruServer;

        public void Run()
        {
            if (_Options.PIDFile != null)
            {
                if (!File.Exists(_Options.PIDFile))
                {
                    _Logger.LogLine("Creating PID file...");
                    Process currentProcess = Process.GetCurrentProcess();
                    File.WriteAllText(_Options.PIDFile, currentProcess.Id.ToString(), Encoding.ASCII); //TODO Use FileStream
                    if (_Options.User != null)
                    {
                        _Logger.LogLine("Setting PID file owner to '{0}'...", _Options.User);
                        ServerHelper.ChOwn(_Options.PIDFile, _Options.User);
                    }
                }
                else throw new Exception("PIDFile exists, make sure no 2nd instance is running and delete the PID file");
            }

            _Logger.LogLine("Loading certificate...");
            //string certificateFile = _Options.Certificate ?? Path.Combine(_Options.Location, "cert.pfx");
            string certificateFile = Path.Combine(_Options.Location, "cert.pfx");
            //X509Certificate2 sCertificate = new X509Certificate2(certificateFile, _Options.CertificatePassword);
            X509Certificate2 sCertificate = new X509Certificate2(certificateFile, "sharpbooru");

            _Logger.LogLine("Loading booru database...");
            ServerBooru booru = new ServerBooru(_Options.Location);

            _Logger.LogLine("Creating server instance...");
            _BooruServer = new BooruServer(booru, _Logger, sCertificate, _Options.Port);

            _Logger.LogLine("Starting BroadcastListener...");
            _BroadcastListenerThread = new Thread(() =>
                {
                    while (true)
                        try
                        {
                            string booruName = booru.GetMiscOption<string>(ServerBooru.MiscOption.BooruName);
                            Broadcaster.ListenForBroadcast(booruName, _Options.Port);
                        }
                        catch (Exception ex)
                        {
                            if (_BroadcastListenerThreadRunning)
                                _Logger.LogException("BroadcastListener", ex);
                        }
                });
            _BroadcastListenerThread.Start();

            EventWaitHandle waitEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
            _Logger.LogLine("Registering CtrlC handler...");
            Console.CancelKeyPress += (sender, e) => Cancel(waitEvent);
            if (Helper.IsUnix())
            {
                _Logger.LogLine("Registering SIGTERM handler...");
                ServerHelper.SetupSignal(Signum.SIGTERM, () => Cancel(waitEvent));
            }

            _Logger.LogLine("Starting server (ProtocolVersion = {0})...", BooruProtocol.ProtocolVersion);
            _BooruServer.Start();

            if (_Options.User != null)
            {
                _Logger.LogLine("Changing UID to user '{0}'...", _Options.User);
                try { ServerHelper.SetUID(_Options.User); }
                catch (Exception ex) { _Logger.LogException("SetUID", ex); }
            }

            _Logger.LogLine("Server startup finished");
            waitEvent.WaitOne(); //Wait for Cancel to finish
        }

        private bool _CancelRunned = false;
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Cancel(EventWaitHandle WaitEvent)
        {
            if (!_CancelRunned)
            {
                _Logger.LogLine("Stopping BroadcastListener...");
                _BroadcastListenerThreadRunning = false;
                _BroadcastListenerThread.Abort();
                _Logger.LogLine("Stopping server and waiting for clients to finish...");
                //_ServerBroadcaster.Stop();
                _BooruServer.Stop();
                _Logger.LogLine("Disposing server and closing database connection...");
                _BooruServer.Dispose();
                WaitEvent.Set();
                if (_Options.PIDFile != null)
                {
                    _Logger.LogLine("Removing PID file...");
                    try { File.Delete(_Options.PIDFile); }
                    catch (Exception ex) { _Logger.LogException("RemovePIDFile", ex); }
                }
                _CancelRunned = true;
            }
        }
    }
}
