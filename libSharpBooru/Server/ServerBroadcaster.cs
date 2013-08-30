using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace TA.SharpBooru.Server
{
    public class ServerBroadcaster
    {
        public const ushort Port = 24024;

        private Socket _Socket;
        private EndPoint _EndPoint;
        private byte[] _Datagram;
        private bool _bcThreadRunning = false;

        public ServerBroadcaster(Booru Booru, ushort Port)
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
}