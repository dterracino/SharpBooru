using System;
using System.Threading;
using Amib.Threading;

namespace TA.SharpBooru.Server
{
    public abstract class Server
    {
        private Logger _Logger = Logger.Null;
        private SmartThreadPool _ThreadPool = new SmartThreadPool();
        protected Thread _ConnectClientThread = null;
        protected bool _ConnectClientThreadRunning = false;
        protected object _StartStopLock = new object();

        public SmartThreadPool ThreadPool { get { return _ThreadPool; } }
        public Logger Logger
        {
            get { return _Logger; }
            set { _Logger = value ?? Logger.Null; }
        }

        public abstract string ServerName { get; }
        public virtual string ServerInfo { get { return null; } }

        public abstract object ConnectClient();
        public abstract void HandleClient(object Client);
        public abstract void StartListener();
        public abstract void StopListener();

        private string ServerString
        {
            get
            {
                string serverInfo = ServerInfo ?? string.Empty;
                string serverName = ServerName ?? string.Empty;
                return string.Format("{0} [{1}]", serverName.Trim(), serverInfo.Trim());
            }
        }

        public void Start()
        {
            lock (_StartStopLock)
                if (!_ConnectClientThreadRunning)
                {
                    _ConnectClientThread = new Thread(_HandlerStage1);
                    _ConnectClientThreadRunning = true;
                    StartListener();
                    _ConnectClientThread.Start();
                    Logger.LogLine("{0} running...", ServerString);
                }
                else throw new Exception("Already running");
        }

        public void Stop(int Timeout = Timeout.Infinite)
        {
            lock (_StartStopLock)
                if (_ConnectClientThreadRunning)
                {
                    _ConnectClientThreadRunning = false;
                    StopListener();
                    if (Timeout > 0)
                        _ThreadPool.WaitForIdle(Timeout);
                    else if (Timeout < 0)
                        _ThreadPool.WaitForIdle();
                    _ThreadPool.Cancel(true);
                    _ConnectClientThread.Abort();
                    Logger.LogLine("{0} stopped...", ServerString);
                }
                else throw new Exception("Not running");
        }

        private void _HandlerStage1()
        {
            while (_ConnectClientThreadRunning)
            {
                object client = null;
                try
                {
                    client = ConnectClient();
                    if (_ConnectClientThreadRunning)
                        _ThreadPool.QueueWorkItem(new WorkItemCallback(_HandlerStage2), client);
                }
                catch (Exception ex)
                {
                    if (!_ConnectClientThreadRunning)
                        break; //Ignore exception
                    else HandleException(client, ex);
                }
            }
        }

        private object _HandlerStage2(object Client)
        {
            try { HandleClient(Client); }
            catch (Exception ex)
            {
                if (!_ConnectClientThreadRunning)
                    return null; //Ignore exception
                else HandleException(Client, ex);
            }
            return null;
        }

        public virtual void HandleException(object Client, Exception Ex) { Logger.LogException(ServerName, Ex); }
    }
}