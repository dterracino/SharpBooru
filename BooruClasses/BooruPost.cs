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
        
        //TODO Add ProtoMember attributes
        public Size Size;
        public byte[] Thumbnail;

        public string Comment;
        public DateTime CreationDate;
        public ushort EditCount;
        public byte Rating;
        public short Score;
        public string Source;
        public ushort ViewCount;
        public bool Private;

        public List<BooruTag> Tags;
    }
}