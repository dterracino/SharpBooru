namespace TA.SharpBooru.NetIO.Packets.CorePackets
{
    public class Packet5_Encryption : Packet
    {
        public byte[] Key { get; set; }

        public override ushort PacketID { get { return 5; } }

        protected override void ToWriter(ReaderWriter Writer) { Writer.Write(Key, true); }

        protected override void FromReader(ReaderWriter Reader) { Key = Reader.ReadBytes(); }

        public override void Dispose() { }
    }
}