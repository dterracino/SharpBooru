namespace TA.SharpBooru.NetIO.Packets.BooruPackets
{
    public class Packet22_Search : Packet
    {
        public string SearchExpression { get; set; }

        public override ushort PacketID { get { return 22; } }

        protected override void ToWriter(ReaderWriter Writer) { Writer.Write(SearchExpression, true); }

        protected override void FromReader(ReaderWriter Reader) { SearchExpression = Reader.ReadString(); }

        public override void Dispose() { }
    }
}