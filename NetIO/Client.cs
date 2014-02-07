using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using TA.SharpBooru.NetIO.Packets;

namespace TA.SharpBooru.NetIO
{
    public class Client
    {
        private class ResponseWaiter : IDisposable
        {
            public ManualResetEventSlim WaitEvent = new ManualResetEventSlim(false);
            public Packet Response = null;
            public uint RequestID;

            public ResponseWaiter(uint RequestID) { this.RequestID = RequestID; }

            public void Dispose()
            {
                Response.Dispose();
                WaitEvent.Dispose();
            }
        }

        private Logger _Logger;
        private List<ResponseWaiter> _Waiters;
        private TcpClient _Client;
        private Stream _Stream;

        public Client(IPEndPoint EndPoint, Logger Logger = null)
        {
            _Logger = Logger;
            _Waiters = new List<ResponseWaiter>();
            _Client = new TcpClient();
            _Client.Connect(EndPoint);
        }

        private void ProgressResponse(uint RequestID, Packet response)
        {
            lock (_Waiters)
                for (int i = 0; i < _Waiters.Count; i++)
                    if (_Waiters[i].RequestID == RequestID)
                    {
                        _Waiters[i].Response = response;
                        _Waiters[i].WaitEvent.Set();
                        _Waiters.RemoveAt(i);
                        return;
                    }
            throw new ProtocolViolationException("Nobody is waiting for this RequestID");
        }

        public Packet DoRequest(Packet RequestPacket, TimeSpan? Timeout = null)
        {
            //TODO Test connection before sending and reconnect if needed
            uint requestID = RequestPacket.ToStream(_Stream);
            using (ResponseWaiter waiter = new ResponseWaiter(requestID))
            {
                lock (_Waiters)
                    _Waiters.Add(waiter);
                if (Timeout.HasValue)
                    waiter.WaitEvent.Wait(Timeout.Value);
                else waiter.WaitEvent.Wait();
                if (waiter.Response is Packet1_Exception)
                    throw ((Packet1_Exception)waiter.Response).Exception;
                else return waiter.Response;
            }
        }
    }
}