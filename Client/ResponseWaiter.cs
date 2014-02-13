using System;
using System.Threading;
using TA.SharpBooru.NetIO.Packets;

namespace TA.SharpBooru.Client
{
    public class ResponseWaiter : IDisposable
    {
        public ManualResetEventSlim WaitEvent = new ManualResetEventSlim(false);
        public Packet Response = null;
        public uint RequestID;

        public ResponseWaiter(uint RequestID) { this.RequestID = RequestID; }

        public void Dispose() { WaitEvent.Dispose(); }
    }
}