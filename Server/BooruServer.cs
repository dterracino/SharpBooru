using System;
using System.Net;
using System.Net.Sockets;

namespace TA.SharpBooru.Server
{
    public class BooruServer : Server, IDisposable
    {
        public const ushort ProtocolVersion = 50;

        private ServerBooru _Booru;
        private TcpListener _Listener;

        public ServerBooru Booru { get { return _Booru; } }
        public ushort Port { get { return (ushort)(_Listener.LocalEndpoint as IPEndPoint).Port; } }

        public BooruServer(ServerBooru Booru, Logger Logger, IPEndPoint LocalEndPoint)
        {
            _Booru = Booru;
            this.Logger = Logger;
            _Listener = new TcpListener(LocalEndPoint);
        }

        public override string ServerName { get { return "NetIO Server"; } }
        public override string ServerInfo { get { return "LocalEndPoint " + _Listener.LocalEndpoint.ToString(); } }

        public override object ConnectClient() { return _Listener.AcceptTcpClient(); }

        public override void HandleClient(object Client)
        {
            using (ClientHandler handler = new ClientHandler(_Booru, Client as TcpClient))
                handler.Handle();
        }

        public override void StartListener() { _Listener.Start(); }

        public override void StopListener() { _Listener.Stop(); }

        public void Dispose() { Stop(3000); }
    }
}