using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace TA.SharpBooru
{
    public abstract class SocketListener : IDisposable
    {
        private Socket _Socket;
        private Thread _ListenerThread;
        private bool _IsRunning;

        public SocketListener(Socket Socket)
        {
            _Socket = Socket;
            ThreadStart ts = new ThreadStart(() =>
                {
                    while (_IsRunning)
                        try
                        {
                            Socket sock = _Socket.Accept();

                        }
                        catch
                        {
                            if (_IsRunning)
                                throw;
                        }
                });
            _ListenerThread = new Thread(ts) { Name = "Listener" };
        }

        protected abstract void HandleClient();

        public virtual void Start()
        {
            _Socket.Listen(40);
            _IsRunning = true;
            _ListenerThread.Start();
        }

        public virtual void Dispose()
        {
            _IsRunning = false;
            _Socket.Dispose();
            if (!_ListenerThread.Join(2000))
                _ListenerThread.Abort();
        }
    }
}
