namespace TA.SharpBooru.NetIO.Packets.BooruPackets
{
    public class Packet26_SearchImg : Packet
    {
        public byte[] ImageHash { get; set; }

        public override ushort PacketID { get { return 26; } }

        protected override void ToWriter(ReaderWriter Writer) { Writer.Write(ImageHash, true); }

        protected override void FromReader(ReaderWriter Reader) { ImageHash = Reader.ReadBytes(); }

        public override void Dispose() { }
    }
}
