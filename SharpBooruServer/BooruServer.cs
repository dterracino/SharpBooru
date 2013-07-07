﻿using System;
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
        private Thread _ListenerThread;
        private X509Certificate _Certificate;
        private Booru _Booru;
        private bool _ListenerRunning = false;

        public Booru Booru { get { return _Booru; } }
        public X509Certificate Certificate { get { return _Certificate; } }

        public BooruServer(Booru Booru, X509Certificate Certificate, int Port = 2400)
        {
            _Booru = Booru;
            _Certificate = Certificate;
            _Listener = new TcpListener(IPAddress.Any, 2400);
            _ThreadPool = new SmartThreadPool();
            _ThreadPool.MaxThreads = int.MaxValue;
            ThreadStart listenerThreadStart = () =>
                {
                    while (_ListenerRunning)
                        try
                        {
                            TcpClient client = _Listener.AcceptTcpClient();
                            ClientHandler handler = new ClientHandler(this, client);
                            handler.Queue(_ThreadPool);
                        }
                        catch { /* TODO Log error */ }
                };
            _ListenerThread = new Thread(listenerThreadStart);
        }

        public void Start()
        {
            _Listener.Start();
            _ListenerRunning = true;
            _ListenerThread.Start();
        }

        public void Stop()
        {
            _Listener.Stop();
            _ListenerRunning = false;
        }
    }
}
