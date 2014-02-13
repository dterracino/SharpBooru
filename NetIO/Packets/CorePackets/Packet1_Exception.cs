using System;
using System.IO;

namespace TA.SharpBooru.NetIO.Packets.CorePackets
{
    public class Packet1_Exception : Packet
    {
        public Exception Exception { get; set; }

        public override ushort PacketID { get { return 1; } }

        protected override void FromReader(ReaderWriter Reader) { Exception = new Exception(Reader.ReadString()); }

        protected override void ToWriter(ReaderWriter Writer) { Writer.Write(Exception.Message, true); }

        public override void Dispose() { }
    }
}