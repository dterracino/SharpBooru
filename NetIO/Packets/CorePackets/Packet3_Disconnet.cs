namespace TA.SharpBooru.NetIO.Packets.CorePackets
{
    public class Packet3_Disconnect : Packet
    {
        public override ushort PacketID { get { return 3; } }

        protected override void ToWriter(ReaderWriter Writer) { }

        protected override void FromReader(ReaderWriter Reader) { }

        public override void Dispose() { }
    }
}