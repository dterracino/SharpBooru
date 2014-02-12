using System;
using System.IO;
using System.Threading;
using System.Runtime.CompilerServices;
using Mono.Unix.Native;

namespace TA.SharpBooru.Server
{
    public class Program
    {
        public static void subMain(Options options, Logger logger)
        {
            Program program = new Program(options, logger);
            program.Run();
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
            /*
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
            */

            if (_Options.Location == null)
                throw new ArgumentNullException("Please provide a booru location");
            else if (!Directory.Exists(_Options.Location))
                throw new DirectoryNotFoundException("Booru location not found");

            _Logger.LogLine("Loading booru database...");
            ServerBooru booru = new ServerBooru(_Options.Location);

            _Logger.LogLine("Creating server instance...");
            ushort port = _Options.Port < 1 ? (ushort)2400 : _Options.Port;
            _BooruServer = new BooruServer(booru, _Logger, port);

            _Logger.LogLine("Starting BroadcastListener...");
            _BroadcastListenerThread = new Thread(() =>
                {
                    while (true)
                        try { Broadcaster.ListenForBroadcast(booru.BooruInfo.BooruName, port); }
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

            if (_Options.Username != null)
            {
                _Logger.LogLine("Changing UID to user '{0}'...", _Options.Username);
                try { ServerHelper.SetUID(_Options.Username); }
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
                _BooruServer.Stop(3000);
                _Logger.LogLine("Disposing server and closing database connection...");
                _BooruServer.Dispose();
                /*
                if (_Options.PIDFile != null)
                {
                    _Logger.LogLine("Removing PID file...");
                    try { File.Delete(_Options.PIDFile); }
                    catch (Exception ex) { _Logger.LogException("RemovePIDFile", ex); }
                }
                */
                _CancelRunned = true;
                WaitEvent.Set();
            }
        }
    }
}
