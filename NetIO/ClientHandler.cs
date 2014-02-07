using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace TA.SharpBooru.NetIO
{
    public class ClientHandler : IDisposable
    {
        private bool _ContinueHandling = true;
        private TcpClient _Client;
        private NetworkStream _NetStream;

        public ClientHandler(TcpClient Client)
        {
            _Client = Client;
            _NetStream = _Client.GetStream();
        }

        public void Dispose()
        {
            _NetStream.Dispose();
            _Client.Close();
        }

        public void Handle()
        {
            while (_ContinueHandling)
            {
                using (ReaderWriter ridReader = new ReaderWriter(_NetStream))
                {
                    Packet packet = Packet.FromStream(_NetStream);
                    switch (
                }
            }
        }
    }
}