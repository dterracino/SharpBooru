namespace TA.SharpBooru.NetIO.Packets
{
    public class Packet2_IsAlive : Packet
    {
        public override ushort PacketID { get { return 2; } }

        protected override void ToWriter(ReaderWriter Writer) { }

        protected override void FromReader(ReaderWriter Reader) { }

        public override void Dispose() { }
    }
}