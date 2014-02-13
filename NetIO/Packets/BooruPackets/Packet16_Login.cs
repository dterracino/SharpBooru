namespace TA.SharpBooru.NetIO.Packets.BooruPackets
{
    public class Packet16_Login : Packet
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public override ushort PacketID { get { return 16; } }

        protected override void ToWriter(ReaderWriter Writer)
        {
            Writer.Write(Username, true);
            Writer.Write(Password, true);
        }

        protected override void FromReader(ReaderWriter Reader)
        {
            Username = Reader.ReadString();
            Password = Reader.ReadString();
        }

        public override void Dispose() { }
    }
}