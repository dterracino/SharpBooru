using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace TA.SharpBooru
{
    public class Broadcaster
    {
        private static byte[] BROADCAST_MAGIC = new byte[] { 0x42, 0x4f, 0x4f, 0x52, 0x55, 0x31, 0x37 };
        private const ushort PORT = 2400;

        public class Response
        {
            public string BooruName;
            public string Hostname;
            public IPEndPoint EndPoint;
        }

        public static Response SearchForServer(ushort Timeout = 1000)
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                IPEndPoint bcEndPoint = new IPEndPoint(IPAddress.Broadcast, PORT);
                socket.SendTo(BROADCAST_MAGIC, bcEndPoint);
            }
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, PORT);
                using (UdpClient client = new UdpClient(clientEndPoint))
                {
                    client.Client.ReceiveTimeout = Timeout;
                    using (MemoryStream datagramStream = new MemoryStream(client.Receive(ref clientEndPoint)))
                    using (BinaryReader reader = new BinaryReader(datagramStream))
                        return new Response
                        {
                            BooruName = reader.ReadString(),
                            Hostname = reader.ReadString(),
                            EndPoint = clientEndPoint //Port correctly set?
                        };
                }
            }
            catch { return null; }
        }

        public static void ListenForBroadcast()
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, PORT);
            using (UdpClient client = new UdpClient(clientEndPoint))
            using (MemoryStream datagramStream = new MemoryStream(client.Receive(ref clientEndPoint)))
            {
                byte[] datagram = datagramStream.ToArray();
                if (datagram.Length == BROADCAST_MAGIC.Length)
                {
                    //TODO Checks
                    //TODO Send Response instance
                    client.Send(new byte[1] { 0x17 }, 1, clientEndPoint);
                }
            }
        }
    }
}