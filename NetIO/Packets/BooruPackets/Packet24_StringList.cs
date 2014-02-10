using System.Collections.Generic;

namespace TA.SharpBooru.NetIO.Packets.BooruPackets
{
    public class Packet24_StringList : Packet
    {
        public List<string> StringList { get; set; }

        public override ushort PacketID { get { return 24; } }

        protected override void ToWriter(ReaderWriter Writer)
        {
            Writer.Write((uint)StringList.Count);
            foreach (string str in StringList)
                Writer.Write(str, true);
        }

        protected override void FromReader(ReaderWriter Reader)
        {
            StringList = new List<string>();
            uint count = Reader.ReadUInt();
            for (uint i = 0; i < count; i++)
                StringList.Add(Reader.ReadString());
        }

        public override void Dispose() {  }
    }
}