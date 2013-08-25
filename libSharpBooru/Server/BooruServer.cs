using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace TA.SharpBooru.Server.Experimental
{
    public class BooruServer : Server
    {
        private TcpListener _Listener;
        private X509Certificate _Certificate;
        private Booru _Booru;
        private Logger _Logger;

        public static readonly uint ServerVersion = 2;
        public override string ServerName { get { return "BooruServer"; } }

        public Booru Booru { get { return _Booru; } }
        public Logger Logger { get { return _Logger; } }
        public X509Certificate Certificate { get { return _Certificate; } }

        public BooruServer(Booru Booru, Logger Logger, X509Certificate Certificate, ushort Port = 2400)
        {
            _Booru = Booru;
            _Logger = Logger;
            _Certificate = Certificate;
            _Listener = new TcpListener(IPAddress.Any, Port);
        }

        public override object ConnectClient() { return _Listener.AcceptTcpClient(); }

        public override void HandleClient(object Client)
        {
            TcpClient client = Client as TcpClient;
            //Handle Client here
        }

        public override bool HandleException(System.Exception Ex)
        {
            _Logger.LogException(ServerName, Ex);
            return true;
        }

        public override void StartListener() { _Listener.Start(); }

        public override void StopListener() { _Listener.Stop(); }
    }
}