using System.IO;
using System.Net;
using System.Net.Sockets;
using TA.SharpBooru.Server;

namespace TA.SharpBooru.Client
{
    public class ServerBroadcast
    {
        public string BooruName;
        public string Hostname;
        public IPAddress IPAddress;
        public ushort Port;

        private ServerBroadcast() { }

        public static ServerBroadcast SearchForServer(int Duration = 3000)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, ServerBroadcaster.Port);
            try
            {
                using (UdpClient client = new UdpClient(endPoint))
                {
                    client.Client.ReceiveTimeout = Duration;
                    using (MemoryStream datagramStream = new MemoryStream(client.Receive(ref endPoint)))
                    using (BinaryReader reader = new BinaryReader(datagramStream))
                        return new ServerBroadcast
                        {
                            BooruName = reader.ReadString(),
                            Hostname = reader.ReadString(),
                            Port = reader.ReadUInt16(),
                            IPAddress = endPoint.Address
                        };
                }
            }
            catch { return null; }
        }
    }
}