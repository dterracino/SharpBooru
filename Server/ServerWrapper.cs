using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Runtime.CompilerServices;
using Mono.Unix.Native;

namespace TA.SharpBooru.Server
{
    public class ServerWrapper
    {
        public ServerWrapper(Logger Logger) { _Logger = Logger; }

        private Logger _Logger;
        //private Thread _BroadcastListenerThread;
        //private bool _BroadcastListenerThreadRunning = true;
        private BooruServer _BooruServer;

        public BooruServer BooruServer { get { return _BooruServer; } }

        public void StartServer(string location, string setuidUser, IPEndPoint localEndPoint, bool waitForExit = true)
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

            if (location == null)
                throw new ArgumentNullException("Please provide a booru location");
            else if (!Directory.Exists(location))
                throw new DirectoryNotFoundException("Booru location not found");

            Updater updater = new Updater(_Logger, location, setuidUser);
            updater.Update();

            _Logger.LogLine("Loading booru database...");
            ServerBooru booru = new ServerBooru(location);

            _Logger.LogLine("Creating server instance...");
            if (localEndPoint.Port < 1)
                localEndPoint.Port = 2400;
            _BooruServer = new BooruServer(booru, _Logger, localEndPoint);

            /*
            _Logger.LogLine("Starting BroadcastListener...");
            _BroadcastListenerThread = new Thread(() =>
                {
                    while (true)
                        try { Broadcaster.ListenForBroadcast(booru.BooruInfo.BooruName, localEndPoint.Port); }
                        catch (Exception ex)
                        {
                            if (_BroadcastListenerThreadRunning)
                                _Logger.LogException("BroadcastListener", ex);
                        }
                });
            _BroadcastListenerThread.Start();
            */

            EventWaitHandle waitEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
            _Logger.LogLine("Registering CtrlC handler...");
            Console.CancelKeyPress += (sender, e) => Cancel(waitEvent);
            if (Helper.IsUnix())
            {
                _Logger.LogLine("Registering SIGTERM handler...");
                ServerHelper.SetupSignal(Signum.SIGTERM, () => Cancel(waitEvent));
            }

            _Logger.LogLine("Starting server V{0}...", Helper.GetVersionMinor());
            _BooruServer.Start();

            if (setuidUser != null)
            {
                _Logger.LogLine("Changing UID to user '{0}'...", setuidUser);
                try { ServerHelper.SetUID(setuidUser); }
                catch (Exception ex) { _Logger.LogException("SetUID", ex); }
            }

            _Logger.LogLine("Server startup finished");

            if (waitForExit)
                waitEvent.WaitOne(); //Wait for Cancel to finish
        }

        private bool _CancelRunned = false;
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Cancel(EventWaitHandle WaitEvent = null)
        {
            if (!_CancelRunned)
            {
                _Logger.LogLine("Stopping BroadcastListener...");
                /*
                _BroadcastListenerThreadRunning = false;
                _BroadcastListenerThread.Abort();
                */
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

                if (WaitEvent != null)
                    WaitEvent.Set();
            }
        }
    }
}
