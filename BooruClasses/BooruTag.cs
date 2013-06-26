using System;
using System.Drawing;
using ProtoBuf;

namespace TA.SharpBooru
{
    [ProtoContract]
    public class BooruTag
    {
        public BooruTag(string Tag) { this.Tag = Tag; }
        public BooruTag(string Tag, string Type, string Description, Color Color)
            : this(Tag)
        {
            this.Type = Type;
            this.Description = Description;
            this.Color = Color;
        }

        [ProtoMember(1)]
        public ulong ID;

        [ProtoMember(2)]
        public string Tag = "unknown";
        [ProtoMember(3)]
        public string Type = "Temporary";
        [ProtoMember(4)]
        public string Description = "Temporary tags";
        [ProtoMember(5)]
        public Color Color = Color.Black;
    }
}
