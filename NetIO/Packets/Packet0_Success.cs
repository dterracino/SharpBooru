namespace TA.SharpBooru.NetIO.Packets
{
    public class Packet0_Success : Packet
    {
        public override ushort PacketID { get { return 0; } }

        protected override void ToWriter(ReaderWriter Writer) { }

        protected override void FromReader(ReaderWriter Reader) { }

        public override void Dispose() { }
    }
}