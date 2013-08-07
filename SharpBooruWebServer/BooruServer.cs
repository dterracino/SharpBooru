using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Specialized;
using Amib.Threading;
using TEAM_ALPHA.SQLBooru.Server.VirtualFileSystem;

namespace TEAM_ALPHA.SQLBooru.Server
{
    public class BooruServer
    {
        private Booru _Booru;
        private Logger _Logger;
        private SmartThreadPool _Pool;
        private HttpListener _Listener;
        private Thread _AcceptConnectionsThread;
        private VFSDirectory _RootDirectory;
        private RequestSecurityManager _RSM;
        private bool _EnableDirectoryListing;
        private CookieManager _CookieManager;
        private BooruUserManager _UserManager;

        public Booru Booru { get { return _Booru; } }
        public Logger Logger { get { return _Logger; } }
        public VFSDirectory RootDirectory { get { return _RootDirectory; } }
        public RequestSecurityManager RSM { get { return _RSM; } }
        public bool EnableDirectoryListing { get { return _EnableDirectoryListing; } set { _EnableDirectoryListing = value; } }
        public BooruUserManager UserManager { get { return _UserManager; } }
        public CookieManager CookieManager { get { return _CookieManager; } }

        public BooruServer(Booru Booru) : this(Booru, null) { }
        public BooruServer(Booru Booru, Logger Logger) : this(Booru, Logger, (List<string>)null, true) { }
        public BooruServer(Booru Booru, Logger Logger, string ListenerPrefix)
            : this(Booru, Logger, new string[1] { ListenerPrefix }.ToList(), true) { }
        public BooruServer(Booru Booru, Logger Logger, string ListenerPrefix, bool EnableRSM)
            : this(Booru, Logger, new string[1] { ListenerPrefix }.ToList(), EnableRSM) { }
        public BooruServer(Booru Booru, Logger Logger, List<string> ListenerPrefixes, bool EnableRSM)
        {
            if (Booru == null)
                throw new ArgumentNullException("Booru");
            else _Booru = Booru;
            if (Logger == null)
                Logger = new Logger();
            else _Logger = Logger;
            _Listener = new HttpListener();
            if (ListenerPrefixes == null)
                ListenerPrefixes = new List<string>();
            if (ListenerPrefixes.Count < 1)
                ListenerPrefixes.Add("http://*:80/");
            ListenerPrefixes.ForEach(x => _Listener.Prefixes.Add(x));
            _Listener.IgnoreWriteExceptions = false; //TODO
            _EnableDirectoryListing = EnableDirectoryListing;
            _Pool = new SmartThreadPool(3000, 500, 10);
            _RootDirectory = new VFSDirectory(string.Empty);
            _RSM = EnableRSM ? new RequestSecurityManager() : null;
            _CookieManager = new CookieManager();
            _UserManager = new BooruUserManager(_Booru);
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
                        catch (Exception ex) { LogException(ex); }
                });
        }

        public void Start()
        {
            if (!_Listener.IsListening)
            {
                InitAcceptConnectionsThread();
                _Listener.Start();
                _AcceptConnectionsThread.Start();
                Logger.LogLine("[c3]Server running...");
            }
        }

        public void Stop() { Stop(true); }
        public void Stop(bool Wait)
        {
            _Listener.Stop();
            if (Wait)
                while (_AcceptConnectionsThread.IsAlive)
                    Thread.Sleep(100);
            Logger.LogLine("[c3]Server stopped...");
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        private void LogException(Exception ex)
        {
            if (ex == null)
                ex = new ArgumentException("Exception not provided");
            Logger.LogLine("[c1]ERROR[cr]: [c7]{0}", ex.Message);
        }

        private object internalHandleRequestS1(object oContext)
        {
            if (!(oContext is HttpListenerContext))
                throw new ArgumentException("oContext is no HttpListenerContext");
            using (Context bContext = new Context(this, oContext as HttpListenerContext))
            {
                Logger.LogLine("Request from [c7]{2}[cr]: {0} {1}", bContext.Method, bContext.InnerContext.Request.Url, bContext.ClientIP);
                try
                {
                    int RSMCode = RSM == null ? 200 : RSM.CheckAllowedConnectionAndReturnHttpCode(bContext.ClientIP);
                    if (!(bContext.Method == "POST" || bContext.Method == "GET"))
                        throw new NotImplementedException(string.Format("HTTP method {0} not implemented", bContext.Method));
                    internalHandleRequestS2(bContext, RSMCode);
                    if (!(bContext.HTTPCode < 400))
                        throw new WebException(string.Format("Returned to [c7]{0}[cr]: {1} - {2}", bContext.ClientIP.ToString(), bContext.HTTPCode, ServerHelper.GetHTTPCodeDescription(bContext.HTTPCode)));
                }
                catch (Exception ex) { LogException(ex); }
                if (RSM != null)
                    RSM.RequestProcessed(bContext.ClientIP);
                return null;
            }
        }

        private void internalHandleRequestS2(Context bContext, int RSMCode)
        {
            Exception executionException = null;
            if (RSMCode == 200)
            {
                VFSFile file = _RootDirectory.GetFile(bContext.RequestPath);
                if (file != null)
                    try { file.Execute(bContext); }
                    catch (Exception ex)
                    {
                        executionException = ex;
                        try { bContext.HTTPCode = 500; }
                        catch { }
                    }
                else bContext.HTTPCode = 404;
            }
            else bContext.HTTPCode = RSMCode;
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