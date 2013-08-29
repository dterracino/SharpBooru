using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

namespace TA.SharpBooru
{
    public class ServerBroadcaster
    {
        public const ushort Port = 24024;

        private Socket _Socket;
        private EndPoint _EndPoint;
        private byte[] _Datagram;
        private bool _bcThreadRunning = false; //

        public ServerBroadcaster(Server.Booru Booru, ushort Port)
        {
            _Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            _EndPoint = new IPEndPoint(IPAddress.Broadcast, ServerBroadcaster.Port);
            using (MemoryStream datagramStream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(datagramStream))
                {
                    writer.Write("BooruName");
                    writer.Write(Environment.MachineName);
                    writer.Write(Port);
                }
                datagramStream.Flush(); //Really needed?
                _Datagram = datagramStream.ToArray();
            }
        }

        public void Broadcast() { _Socket.SendTo(_Datagram, _EndPoint); }

        public void Start(int Interval = 1000)
        {
            if (!_bcThreadRunning)
            {
                _bcThreadRunning = true;
                Thread bcThread = new Thread(() =>
                    {
                        while (_bcThreadRunning)
                        {
                            Broadcast();
                            Thread.Sleep(1000);
                        }
                    });
                bcThread.Start();
            }
            else throw new Exception("Already running");
        }

        public void Stop()
        {
            if (_bcThreadRunning)
                _bcThreadRunning = false;
            else throw new Exception("Not running");
        }
    }

    public class ServerBroadcast
    {
        public string BooruName;
        public string Hostname;
        public IPAddress IPAddress;
        public ushort Port;

        private ServerBroadcast() { }

        public static ServerBroadcast SearchForServer(int Duration = 3000)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, ServerBroadcaster.Port); try
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