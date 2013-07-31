using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Amib.Threading;

namespace TA.SharpBooru.Server
{
    public class BooruServer
    {
        public static readonly uint ServerVersion = 1;

        private TcpListener _Listener;
        private SmartThreadPool _ThreadPool;
        private X509Certificate _Certificate;
        private Booru _Booru;
        private Logger _Logger;
        private bool _ListenerRunning = false;

        public Booru Booru { get { return _Booru; } }
        public Logger Logger { get { return _Logger; } }
        public X509Certificate Certificate { get { return _Certificate; } }

        public BooruServer(Booru Booru, Logger Logger, X509Certificate Certificate, ushort Port = 2400)
        {
            _Booru = Booru;
            _Logger = Logger;
            _Certificate = Certificate;
            _Listener = new TcpListener(IPAddress.Any, Port);
            _ThreadPool = new SmartThreadPool();
            _ThreadPool.MaxThreads = int.MaxValue;
        }

        public void Start()
        {
            _ListenerRunning = true;
            _Listener.Start();
            _Logger.LogLine("Server running...");
            while (_ListenerRunning)
                try
                {
                    TcpClient client = _Listener.AcceptTcpClient();
                    ClientHandler handler = new ClientHandler(this, client);
                    handler.Queue(_ThreadPool);
                }
                catch (Exception ex)
                {
                    if (_ListenerRunning)
                        _Logger.LogException(ex);
                }
        }

        public void Stop()
        {
            _ListenerRunning = false;
            _Listener.Stop();
            _Logger.LogLine("Server stopped...");
        }

        public bool WaitForClients(int Timeout)
        {
            if (Timeout == 0)
                return _ThreadPool.IsIdle;
            for (int i = 0; i < Timeout * 10 || Timeout < 0; i++)
            {
                Thread.Sleep(100);
                if (_ThreadPool.IsIdle)
                    return true;
            }
            return false;
        }
    }
}