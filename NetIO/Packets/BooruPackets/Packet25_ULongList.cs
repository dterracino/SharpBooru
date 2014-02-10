using System.Collections.Generic;

namespace TA.SharpBooru.NetIO.Packets.BooruPackets
{
    public class Packet25_ULongList : Packet
    {
        public List<ulong> ULongList { get; set; }

        public override ushort PacketID { get { return 25; } }

        protected override void ToWriter(ReaderWriter Writer)
        {
            Writer.Write((uint)ULongList.Count);
            foreach (ulong ul in ULongList)
                Writer.Write(ul);
        }

        protected override void FromReader(ReaderWriter Reader)
        {
            ULongList = new List<ulong>();
            uint count = Reader.ReadUInt();
            for (uint i = 0; i < count; i++)
                ULongList.Add(Reader.ReadULong());
        }

        public override void Dispose() { }
    }
}