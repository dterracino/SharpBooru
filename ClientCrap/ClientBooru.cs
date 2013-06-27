using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;

namespace TA.SharpBooru.Server
{
    public class ClientBooru
    {
        private byte[] _SessionID;

        private IPEndPoint _EndPoint;
        string _Username, _Password;
        private TcpClient _Client;

        private NetworkStream _NetworkStream;
        private SslStream _SSLStream;

        public ClientBooru(string Server, ushort Port, string Username, string Password)
        {
            _Client = new TcpClient();
            IPAddress address = null;
            if (!IPAddress.TryParse(Server, out address))
            {
                IPHostEntry entry = Dns.GetHostEntry(Server);
                if (entry.AddressList.Length > 1)
                {
                    int randomIndex = (new Random()).Next(0, entry.AddressList.Length);
                    address = entry.AddressList[randomIndex];
                }
                else address = entry.AddressList[0];
            }
            _EndPoint = new IPEndPoint(address, Port);
            Connect();
        }

        private void Connect()
        {
            if (!_Client.Connected)
            {
                _Client.Connect(_EndPoint);
                _NetworkStream = _Client.GetStream();
                _SSLStream = new SslStream(_NetworkStream, true, delegate { return true; });
                _SSLStream.AuthenticateAsClient("SharpBooruServer");
            }
        }

        private object RequestResource(BooruProtocol.Command Command, object TargetObject, object Payload)
        {
            return null;
        }
    }
}