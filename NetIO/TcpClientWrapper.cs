using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace TA.SharpBooru.NetIO
{
    public class TcpClientWrapper
    {
        private TcpClient _Client;

        private Stack<Stream> _StreamsRead = new Stack<Stream>();
        private Stack<Stream> _StreamsWrite = new Stack<Stream>();

        public TcpClientWrapper(TcpClient Client)
        {
            _Client = Client;
            NetworkStream ns = _Client.GetStream();
            _StreamsRead.Push(ns);
            _StreamsWrite.Push(ns);
        }

        public TcpClient Client { get { return _Client; } }
        public Stream ReadStream { get { return _StreamsRead.Peek(); } }
        public Stream WriteStream { get { return _StreamsWrite.Peek(); } }
        public ReaderWriter Reader { get { return new ReaderWriter(ReadStream); } }
        public ReaderWriter Writer { get { return new ReaderWriter(WriteStream); } }
    }
}