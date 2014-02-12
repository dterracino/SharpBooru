namespace TA.SharpBooru.NetIO.Packets.BooruPackets
{
    public class Packet17_GetResource : Packet
    {
        public enum ResourceType : byte { Post, Tag, Image, Thumbnail }

        public ulong ID { get; set; }
        public string Name { get; set; }
        public ResourceType Type { get; set; }

        public override ushort PacketID { get { return 17; } }

        protected override void ToWriter(ReaderWriter Writer)
        {
            Writer.Write((byte)Type);
            if (Name != null)
            {
                Writer.Write(ulong.MaxValue);
                Writer.Write(Name, true);
            }
            else Writer.Write(ID);
        }

        protected override void FromReader(ReaderWriter Reader)
        {
            Type = (ResourceType)Reader.ReadByte();
            ID = Reader.ReadULong();
            if (ID == ulong.MaxValue)
                Name = Reader.ReadString();
        }

        public override void Dispose() { }
    }
}