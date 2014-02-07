using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace TA.SharpBooru.NetIO
{
    public class Server : TA.SharpBooru.Server.Server
    {
        private TcpListener _Listener;

        public override string ServerName { get { return "NetIO Server"; } }
        public override string ServerInfo { get { return "LocalEndPoint " + _Listener.LocalEndpoint.ToString(); } }

        public override object ConnectClient() { return _Listener.AcceptTcpClient(); }

        public override void HandleClient(object Client)
        {
            using (ClientHandler handler = new ClientHandler(Client as TcpClient))
                handler.Handle();
        }

        public override void StartListener() { _Listener.Start(); }

        public override void StopListener() { _Listener.Stop(); }
    }
}