using System;

namespace TA.SharpBooru.NetIO.Packets.BooruPackets
{
    public class Packet20_EditResource : Packet
    {
        public enum ResourceType : byte { Post, Tag, Image }

        public ulong ID { get; set; }
        public ResourceType Type { get; set; }
        public BooruResource Resource { get; set; }

        public override ushort PacketID { get { return 20; } }

        protected override void ToWriter(ReaderWriter Writer)
        {
            Writer.Write(ID);
            Writer.Write((byte)Type);
            Resource.ToWriter(Writer);
        }

        protected override void FromReader(ReaderWriter Reader)
        {
            ID = Reader.ReadULong();
            Type = (ResourceType)Reader.ReadByte();
            switch (Type)
            {
                case ResourceType.Post: Resource = BooruPost.FromReader(Reader); break;
                case ResourceType.Tag: Resource = BooruTag.FromReader(Reader); break;
                case ResourceType.Image: Resource = BooruImage.FromReader(Reader); break;
            }
        }

        public override void Dispose()
        {
            if (Resource is IDisposable)
                ((IDisposable)Resource).Dispose();
        }
    }
}