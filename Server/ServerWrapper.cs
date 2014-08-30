using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Threading;
using System.Runtime.CompilerServices;
using Mono.Unix.Native;
using TA.SharpBooru.Server.WebServer;

namespace TA.SharpBooru.Server
{
    public class ServerWrapper
    {
        public ServerWrapper(Logger Logger) { _Logger = Logger; }

        private Logger _Logger;
        private BooruServer _BooruServer;
        private BooruWebServer _BooruWebServer;

        public BooruServer BooruServer { get { return _BooruServer; } }
        public BooruWebServer BooruWebServer { get { return _BooruWebServer; } }

        public void StartServer(string location, bool waitForExit = true)
        {
            if (location == null)
                throw new ArgumentNullException("Please provide a booru location");
            else if (!Directory.Exists(location))
                throw new DirectoryNotFoundException("Booru location not found");

            XmlDocument config = new XmlDocument();
            config.Load(Path.Combine(location, "config.xml"));
            XmlNode booruConfigNode = config.SelectSingleNode("/BooruConfig");
            string setuidUser = booruConfigNode.SelectSingleNode("User").InnerText;
            XmlNode booruServerNode = booruConfigNode.SelectSingleNode("BooruServer");
            XmlNode webServerNode = booruConfigNode.SelectSingleNode("WebServer");
            bool enableBooruServer = Convert.ToBoolean(booruServerNode.Attributes["enable"].InnerText);
            IPEndPoint bsLocalEP = new IPEndPoint(
                IPAddress.Parse(booruServerNode.SelectSingleNode("ListenAddress").InnerText),
                Convert.ToUInt16(booruServerNode.SelectSingleNode("Port").InnerText));
            bool enableWebServer = Convert.ToBoolean(webServerNode.Attributes["enable"].InnerText);
            IPEndPoint wsLocalEP = new IPEndPoint(
                IPAddress.Parse(webServerNode.SelectSingleNode("ListenAddress").InnerText),
                Convert.ToUInt16(webServerNode.SelectSingleNode("Port").InnerText));

            if (!enableBooruServer && !enableWebServer)
                throw new ArgumentException("No server enabled");

            Updater updater = new Updater(_Logger, location, setuidUser);
            updater.Update();

            _Logger.LogLine("Loading booru database...");
            ServerBooru booru = new ServerBooru(location);

            _Logger.LogLine("Creating server instance(s)...");
            if (enableBooruServer)
            _BooruServer = new BooruServer(booru, _Logger, bsLocalEP);
            if (enableWebServer)
            {
                string prefix = "http://*:" + wsLocalEP.Port + "/";
                _BooruWebServer = new BooruWebServer(booru, _Logger, prefix);
            }

            EventWaitHandle waitEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
            _Logger.LogLine("Registering CtrlC handler...");
            Console.CancelKeyPress += (sender, e) => Cancel(waitEvent);
            if (Helper.IsUnix())
            {
                _Logger.LogLine("Registering SIGTERM handler...");
                ServerHelper.SetupSignal(Signum.SIGTERM, () => Cancel(waitEvent));
            }

            if (enableBooruServer)
            {
                _Logger.LogLine("Starting server V{0}...", Helper.GetVersionMinor());
                _BooruServer.Start();
            }

            if (enableWebServer)
            {
                _Logger.LogLine("Starting webserver...");
                _BooruWebServer.Start();
            }

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
                if (_BooruWebServer != null)
                {
                    _Logger.LogLine("Stopping webserver...");
                    _BooruWebServer.Stop(true);
                }
                if (_BooruServer != null)
                {
                    _Logger.LogLine("Stopping server and waiting for clients to finish...");
                    _BooruServer.Stop(3000);
                    _Logger.LogLine("Disposing server and closing database connection...");
                    _BooruServer.Dispose();
                }
                _CancelRunned = true;

                if (WaitEvent != null)
                    WaitEvent.Set();
            }
        }
    }
}
