using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

namespace TEAM_ALPHA.SharpBooru
{
    [Serializable]
    public class BooruPost : IDisposable
    {
        public uint ID;

        public Size Size;
        public MemoryStream Image;
        public MemoryStream CompareImage;
        public MemoryStream Thumbnail;

        public string Comment;
        public DateTime CreationDate;
        public ushort EditCount;
        public byte Rating;
        public short Score;
        public string Source;
        public ushort ViewCount;
        public bool Private;

        public List<BooruTag> Tags;

        public void Dispose()
        {
            Image.Dispose();
            CompareImage.Dispose();
            Thumbnail.Dispose();
        }
    }
}