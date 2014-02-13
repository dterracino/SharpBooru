namespace TA.SharpBooru.NetIO.Packets.BooruPackets
{
    public class Packet18_GetAllTags : Packet
    {
        public override ushort PacketID { get { return 18; } }

        protected override void ToWriter(ReaderWriter Writer) { }

        protected override void FromReader(ReaderWriter Reader) { }

        public override void Dispose() { }
    }
}