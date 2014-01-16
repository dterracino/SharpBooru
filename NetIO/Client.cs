using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

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

        private Stream stream;
        private List<ResponseWaiter> waiters = new List<ResponseWaiter>();

        public Client()
        {
        }

        private void ProgressResponse(Packet response)
        {
            lock (waiters)
                for (int i = 0; i < waiters.Count; i++)
                    if (waiters[i].RequestID == response.RequestID)
                    {
                        waiters[i].Response = response;
                        waiters[i].WaitEvent.Set();
                        waiters.RemoveAt(i);
                        return;
                    }
            throw new ProtocolViolationException("Nobody is waiting for this RequestID");
        }

        public Packet DoRequest(Packet RequestPacket, TimeSpan? Timeout = null)
        {
            RequestPacket.ToStream(stream);
            using (ResponseWaiter waiter = new ResponseWaiter(RequestPacket.RequestID))
            {
                lock (waiters)
                    waiters.Add(waiter);
                if (Timeout.HasValue)
                    waiter.WaitEvent.Wait(Timeout.Value);
                else waiter.WaitEvent.Wait();
                //TODO HANDLE EXCEPTION PACKET
                return waiter.Response;
            }
        }
    }
}