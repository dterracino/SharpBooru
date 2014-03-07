namespace TA.SharpBooru.NetIO.Packets.BooruPackets
{
    public class Packet28_PubKeyLogin : Packet
    {
        public byte[] Modulus { get; set; }
        public byte[] Exponent { get; set; }
        public byte[] Signature { get; set; }

        public override ushort PacketID { get { return 28; } }

        protected override void ToWriter(ReaderWriter Writer)
        {
            Writer.Write(Modulus, true);
            Writer.Write(Exponent, true);
            Writer.Write(Signature, true);
        }

        protected override void FromReader(ReaderWriter Reader)
        {
            Modulus = Reader.ReadBytes();
            Exponent = Reader.ReadBytes();
            Signature = Reader.ReadBytes();
        }

        public override void Dispose() { }
    }
}