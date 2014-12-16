using System;
using System.Threading;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace TA.SharpBooru.Server
{
    public class SocketListener : IDisposable
    {
        private Socket _Socket;
        private Thread _ListenerThread;
        private bool _IsRunning;
        private X509Certificate2 _Certificate;

        public SocketListener(Socket Socket, X509Certificate2 Certificate)
            : this(Socket)
        {
            _Certificate = Certificate;
        }

        public SocketListener(Socket Socket)
        {
            _Socket = Socket;
            ThreadStart ts = new ThreadStart(() =>
                {
                    while (_IsRunning)
                        try
                        {
                            Socket sock = _Socket.Accept();
                            if (SocketAccepted != null)
                                SocketAccepted(sock);
                            else sock.Dispose();
                        }
                        catch
                        {
                            if (_IsRunning)
                                throw;
                        }
                });
            _ListenerThread = new Thread(ts) { Name = "Listener" };
        }

        public void Start()
        {
            _Socket.Listen(40);
            _IsRunning = true;
            _ListenerThread.Start();
        }

        public void Dispose()
        {
            _IsRunning = false;
            _Socket.Dispose();
            if (!_ListenerThread.Join(2000))
                _ListenerThread.Abort();
        }

        public delegate void SocketAcceptedDelegate(Socket Socket);
        public event SocketAcceptedDelegate SocketAccepted;
    }
}
