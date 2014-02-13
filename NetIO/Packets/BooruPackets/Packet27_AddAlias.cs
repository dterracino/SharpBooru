namespace TA.SharpBooru.NetIO.Packets.BooruPackets
{
    public class Packet27_AddAlias : Packet
    {
        public string Alias { get; set; }
        public ulong TagID { get; set; }

        public override ushort PacketID { get { return 27; } }

        protected override void ToWriter(ReaderWriter Writer)
        {
            Writer.Write(Alias, true);
            Writer.Write(TagID);
        }

        protected override void FromReader(ReaderWriter Reader)
        {
            Alias = Reader.ReadString();
            TagID = Reader.ReadULong();
        }

        public override void Dispose() { }
    }
}