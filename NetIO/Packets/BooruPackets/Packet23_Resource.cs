using System;

namespace TA.SharpBooru.NetIO.Packets.BooruPackets
{
    public class Packet23_Resource : Packet
    {
        public enum ResourceType : byte { Post, Tag, Image, Thumbnail, User }

        public BooruResource Resource { get; set; }
        public ResourceType Type { get; set; }

        public override ushort PacketID { get { return 23; } }

        protected override void ToWriter(ReaderWriter Writer)
        {
            Writer.Write((byte)Type);
            Resource.ToWriter(Writer);
        }

        protected override void FromReader(ReaderWriter Reader)
        {
            Type = (ResourceType)Reader.ReadByte();
            switch (Type)
            {
                case ResourceType.Post: Resource = BooruPost.FromReader(Reader); break;
                case ResourceType.Tag: Resource = BooruTag.FromReader(Reader); break;
                case ResourceType.Image:
                case ResourceType.Thumbnail: Resource = BooruImage.FromReader(Reader); break;
                case ResourceType.User: Resource = BooruUser.FromReader(Reader); break;
            }
        }

        public override void Dispose()
        {
            if (Resource is IDisposable)
                ((IDisposable)Resource).Dispose();
        }
    }
}