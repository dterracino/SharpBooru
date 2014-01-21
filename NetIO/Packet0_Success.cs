using System;
using System.IO;

namespace TA.SharpBooru.NetIO
{
    public class Packet0_Success : Packet
    {
        public override ushort PacketID { get { return 0; } }

        public override void FromReader(ReaderWriter Reader) { }

        public override void ToWriter(ReaderWriter Writer) { WritePacketHeader(Writer, 0); }

        public override void Dispose() { }
    }
}