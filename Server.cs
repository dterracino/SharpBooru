using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Amib.Threading;
using ProtoBuf;

namespace TA.SharpBooru.Server
{
    public class Server
    {
        private class ClientHandler
        {
            private TcpClient _Client;
            private SslStream _SSLStream;
            private bool _LoggedIn;

            public ClientHandler(TcpClient Client) { _Client = Client; }

            public void Handle()
            {
                _SSLStream = new SslStream(_Client.GetStream(), true);
                _SSLStream.AuthenticateAsServer(new X509Certificate("ServerCertificate.pfx", "sharpbooru"), false, System.Security.Authentication.SslProtocols.Tls, false);
                for (int i = 0; i < 3; i++)
                {
                    var loginPacket = BooruProtocol.ReceiveRequest<BooruProtocol.Request<string[]>>(_SSLStream);
                    //TODO Check login credentials / Event
                    _LoggedIn = true;
                    BooruProtocol.SendResponse<bool>(_SSLStream, new BooruProtocol.Response<bool>() { ErrorCode = BooruProtocol.ErrorCode.Success });
                    if (_LoggedIn)
                    {
                        return;
                    }
                }
            }
        }

        public Server()
        {

        }

        private TcpListener _Listener;
        private SmartThreadPool _ThreadPool;
    }
}
