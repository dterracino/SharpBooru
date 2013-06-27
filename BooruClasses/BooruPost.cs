using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using ProtoBuf;

namespace TA.SharpBooru
{
    [ProtoContract]
    public class BooruPost
    {
        [ProtoMember(1)]
        public ulong ID;

        [ProtoMember(2)]
        public List<BooruTag> Tags;

        [ProtoMember(3)]
        public bool Private;
        [ProtoMember(4)]
        public DateTime CreationDate;
        [ProtoMember(5)]
        public ushort ViewCount;
        [ProtoMember(6)]
        public ushort EditCount;
        [ProtoMember(7)]
        public short Score;
        [ProtoMember(8)]
        public byte Rating;
        [ProtoMember(9)]
        public string Source;
        [ProtoMember(10)]
        public string Comment;

        [ProtoMember(11)]
        public uint Width;
        [ProtoMember(12)]
        public uint Height;
        [ProtoMember(13)]
        public byte[] Thumbnail;
    }
}