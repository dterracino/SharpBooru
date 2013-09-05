using System.IO;

namespace TA.SharpBooru
{
    public class BooruInfo
    {
        public string BooruName;
        public string BooruCreator;
        public ushort ThumbnailSize;
        public float ThumbnailQuality;

        public void ToWriter(BinaryWriter Writer)
        {
            Writer.Write(BooruName);
            Writer.Write(BooruCreator);
            Writer.Write(ThumbnailSize);
            Writer.Write(ThumbnailQuality);
        }

        public static BooruInfo FromReader(BinaryReader Reader)
        {
            return new BooruInfo()
            {
                BooruName = Reader.ReadString(),
                BooruCreator = Reader.ReadString(),
                ThumbnailSize = Reader.ReadUInt16(),
                ThumbnailQuality = Reader.ReadSingle()
            };
        }
    }
}