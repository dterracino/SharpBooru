namespace TA.SharpBooru.NetIO.Packets.BooruPackets
{
    public class Packet19_DeleteResource : Packet
    {
        public enum ResourceType : byte { Post, Tag, User }

        public ulong ID { get; set; }
        public ResourceType Type { get; set; }

        public override ushort PacketID { get { return 19; } }

        protected override void ToWriter(ReaderWriter Writer)
        {
            Writer.Write((byte)Type);
            Writer.Write(ID);
        }

        protected override void FromReader(ReaderWriter Reader)
        {
            Type = (ResourceType)Reader.ReadByte();
            ID = Reader.ReadULong();
        }

        public override void Dispose() { }
    }
}