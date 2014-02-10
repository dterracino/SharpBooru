using System;

namespace TA.SharpBooru.NetIO.Packets.BooruPackets
{
    public class Packet21_AddResource : Packet
    {
        public enum ResourceType : byte { Post, User, Alias }

        public ResourceType Type { get; set; }
        public BooruResource Resource { get; set; }

        public override ushort PacketID { get { return 21; } }

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