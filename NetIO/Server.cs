using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using TA.SharpBooru.Server;

namespace TA.SharpBooru.NetIO
{
    public class Server : Server
    {
        private List<TcpClient> _Clients = new List<TcpClient>();
        private TcpListener _Listener;

        public override string ServerName { get { return "NetIO Server"; } }
        public override string ServerInfo { get { return "LocalEndPoint " + _Listener.LocalEndpoint.ToString(); } }

        public override object ConnectClient() { return _Listener.AcceptTcpClient(); }

        public override void HandleClient(object Client)
        {
            throw new NotImplementedException();
        }

        public override bool HandleException(object Client, Exception Ex)
        {
            if (Client != null)
                if (Client is TcpClient)
                    _Clients.Remove(Client as TcpClient);
            return false;
        }

        public override void StartListener() { _Listener.Start(); }

        public override void StopListener() { _Listener.Stop(); }
    }
}