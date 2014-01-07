﻿using System;
using System.Threading;
using Amib.Threading;

namespace TA.SharpBooru.Server
{
    public abstract class Server
    {
        private Logger _Logger = null;
        private SmartThreadPool _ThreadPool = new SmartThreadPool();
        protected Thread _ConnectClientThread = null;
        protected bool _ConnectClientThreadRunning = false;
        protected object _StartStopLock = new object();

        public SmartThreadPool ThreadPool { get { return _ThreadPool; } }
        public Logger Logger
        {
            get { return _Logger ?? Logger.Null; }
            set { _Logger = value; }
        }

        public abstract string ServerName { get; }
        public virtual string ServerInfo { get { return null; } }

        public abstract object ConnectClient();
        public abstract void HandleClient(object Client);
        public abstract bool HandleException(Exception Ex);
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
    }
}