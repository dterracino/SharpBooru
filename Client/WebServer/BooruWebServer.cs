using System;
using System.Net;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Amib.Threading;
using TA.SharpBooru.Client.WebServer.VFS;

namespace TA.SharpBooru.Client.WebServer
{
    public class BooruWebServer
    {
        private BooruClient _Booru;
        private Logger _Logger;
        private SmartThreadPool _Pool;
        private HttpListener _Listener;
        private Thread _AcceptConnectionsThread;
        private VFSDirectory _RootDirectory;
        private bool _EnableDirectoryListing;
        //private CookieManager _CookieManager;

        public BooruClient Booru { get { return _Booru; } }
        public Logger Logger { get { return _Logger; } }
        public VFSDirectory RootDirectory { get { return _RootDirectory; } }
        public bool EnableDirectoryListing { get { return _EnableDirectoryListing; } set { _EnableDirectoryListing = value; } }
        //public CookieManager CookieManager { get { return _CookieManager; } }

        public BooruWebServer(BooruClient Booru, Logger Logger) : this(Booru, Logger, "http://*:80/") { }
        public BooruWebServer(BooruClient Booru, Logger Logger, string ListenerPrefix)
            : this(Booru, Logger, new string[1] { ListenerPrefix }.ToList(), true) { }
        public BooruWebServer(BooruClient Booru, Logger Logger, string ListenerPrefix, bool EnableRSM)
            : this(Booru, Logger, new string[1] { ListenerPrefix }.ToList(), EnableRSM) { }
        public BooruWebServer(BooruClient Booru, Logger Logger, List<string> ListenerPrefixes, bool EnableRSM)
        {
            _Logger = Logger;
            if (Booru == null)
                throw new ArgumentNullException("Booru");
            else _Booru = Booru;
            _Listener = new HttpListener();
            if (ListenerPrefixes == null)
                ListenerPrefixes = new List<string>();
            if (ListenerPrefixes.Count < 1)
                ListenerPrefixes.Add("http://*:80/");
            ListenerPrefixes.ForEach(x => _Listener.Prefixes.Add(x));
            _EnableDirectoryListing = EnableDirectoryListing;
            _Pool = new SmartThreadPool(3000, 500, 10);
            _RootDirectory = new VFSDirectory(string.Empty);
            //_CookieManager = new CookieManager();
        }

        private void InitAcceptConnectionsThread()
        {
            _AcceptConnectionsThread = new Thread(() =>
                {
                    while (_Listener.IsListening)
                        try
                        {
                            HttpListenerContext Context = _Listener.GetContext();
                            _Pool.QueueWorkItem(new WorkItemCallback(internalHandleRequestS1), Context);
                        }
                        catch (Exception ex) { Logger.LogException("QueueRequest", ex); }
                });
        }

        public void Start()
        {
            if (!_Listener.IsListening)
            {
                InitAcceptConnectionsThread();
                _Listener.Start();
                _AcceptConnectionsThread.Start();
                Logger.LogLine("Server running...");
            }
        }

        public void Stop() { Stop(true); }
        public void Stop(bool Wait)
        {
            _Listener.Stop();
            if (Wait)
                while (_AcceptConnectionsThread.IsAlive)
                    Thread.Sleep(100);
            Logger.LogLine("Server stopped...");
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        private object internalHandleRequestS1(object oContext)
        {
            if (!(oContext is HttpListenerContext))
                throw new ArgumentException("oContext is no HttpListenerContext");
            using (Context bContext = new Context(this, oContext as HttpListenerContext))
            {
                Logger.LogLine("Request from {2}: {0} {1}", bContext.Method, bContext.InnerContext.Request.Url, bContext.ClientIP);
                try
                {
                    if (!(bContext.Method == "POST" || bContext.Method == "GET"))
                        throw new NotImplementedException(string.Format("HTTP method {0} not implemented", bContext.Method));
                    internalHandleRequestS2(bContext);
                    if (!(bContext.HTTPCode < 400))
                        throw new WebException(string.Format("Returned to [c7]{0}[cr]: {1} - {2}", bContext.ClientIP.ToString(), bContext.HTTPCode, WebserverHelper.GetHTTPCodeDescription(bContext.HTTPCode)));
                }
                catch (Exception ex) { Logger.LogException("HandleRequest", ex); }
                return null;
            }
        }

        private void internalHandleRequestS2(Context bContext)
        {
            Exception executionException = null;
            VFSFile file = _RootDirectory.GetFile(bContext.RequestPath);
            if (file != null)
                try { file.Execute(bContext); }
                catch (Exception ex)
                {
                    executionException = ex;
                    try
                    {
                        var bEx = ex as BooruException;
                        if (bEx != null)
                            switch (bEx.ErrorCode)
                            {
                                case BooruException.ErrorCodes.NoPermission: bContext.HTTPCode = 403; break;
                                case BooruException.ErrorCodes.ResourceNotFound: bContext.HTTPCode = 404; break;
                                default: bContext.HTTPCode = 500; break;
                            }
                        else bContext.HTTPCode = 500;
                    }
                    catch { }
                }
            else bContext.HTTPCode = 404;
            try
            {
                if (!(bContext.HTTPCode < 400))
                {
                    VFSErrorFile errorFile = new VFSErrorFile(bContext.HTTPCode, null);
                    errorFile.Execute(bContext);
                }
            }
            catch { }
            if (executionException != null)
                throw executionException;
        }
    }
}