using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace TA.SharpBooru.Server
{
    public class BooruServer : Server, IDisposable
    {
        public const ushort ProtocolVersion = 50;

        private ServerBooru _Booru;
        private TcpListener _Listener;

        public BooruServer(ServerBooru Booru,Logger Logger, ushort Port) 
        {
            _Booru = Booru;
            this.Logger = Logger;
            _Listener = new TcpListener(new IPEndPoint(IPAddress.Any, Port));
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