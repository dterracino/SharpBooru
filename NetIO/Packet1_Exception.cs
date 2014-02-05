using System;
using System.IO;

namespace TA.SharpBooru.NetIO
{
    public class Packet1_Exception : Packet
    {
        public Exception Exception { get; set; }

        public override ushort PacketID { get { return 1; } }

        public override void FromReader(ReaderWriter Reader) { Exception = new Exception(Reader.ReadString()); }

        public override void ToWriter(ReaderWriter Writer) { Writer.Write(Exception.Message, true); }

        public override void Dispose() { }
    }
}