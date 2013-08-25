using System;
using System.Threading;
using Amib.Threading;

namespace TA.SharpBooru.Server
{
    public abstract class Server
    {
        private SmartThreadPool _ThreadPool = new SmartThreadPool();
        protected Thread _ConnectClientThread = null;
        protected bool _ConnectClientThreadRunning = false;
        protected object _StartStopLock = new object();

        public abstract string ServerName { get; }
        public SmartThreadPool ThreadPool { get { return _ThreadPool; } }

        public abstract object ConnectClient();
        public abstract void HandleClient(object Client);
        public abstract bool HandleException(Exception Ex);
        public abstract void StartListener();
        public abstract void StopListener();

        public void Start()
        {
            lock (_StartStopLock)
                if (!_ConnectClientThreadRunning)
                {
                    _ConnectClientThread = new Thread(_HandlerStage1);
                    _ConnectClientThreadRunning = true;
                    StartListener();
                    _ConnectClientThread.Start();
                }
                else throw new Exception("Already running");
        }

        public void Stop()
        {
            lock (_StartStopLock)
                if (_ConnectClientThreadRunning)
                {
                    _ConnectClientThreadRunning = false;
                    StopListener();
                    _ThreadPool.WaitForIdle(10 * 1000); //Wait max. 9s for idle
                    _ThreadPool.Cancel(true);
                    _ConnectClientThread.Abort();
                }
                else throw new Exception("Not running");
        }

        protected object _HandlerStage2(object Client)
        {
            try { HandleClient(Client); }
            catch (Exception ex)
            {
                if (!_ConnectClientThreadRunning)
                    return null;
                else if (!HandleException(ex))
                    throw ex;
            }
            return null;
        }

        protected void _HandlerStage1()
        {
            while (_ConnectClientThreadRunning)
            {
                try
                {
                    object client = ConnectClient();
                    if (_ConnectClientThreadRunning)
                        _ThreadPool.QueueWorkItem(new WorkItemCallback(_HandlerStage2), client);
                }
                catch (Exception ex)
                {
                    if (!_ConnectClientThreadRunning)
                        break; //Exit the thread silently
                    else if (!HandleException(ex))
                        throw ex;
                }
            }
        }
    }
}