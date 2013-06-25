using System;
using System.Drawing;

namespace TEAM_ALPHA.SharpBooru
{
    public class BooruPost
    {
        public uint ID;

        public Size Size;
        public BooruImage Image;
        public BooruImage Thumbnail;

        public string Comment;
        public DateTime CreationDate;
        public ushort EditCount;
        public byte Rating;
        public short Score;
        public string Source;
        public ushort ViewCount;
        public bool Private;
        public byte[] CompareImage;
    }
}
