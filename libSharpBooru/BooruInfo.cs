using System.IO;

namespace TA.SharpBooru
{
    public class BooruInfo
    {
        public string BooruName;
        public string BooruCreator;
        public ushort ThumbnailSize;

        public void ToWriter(BinaryWriter Writer)
        {
            Writer.Write(BooruName);
            Writer.Write(BooruCreator);
            Writer.Write(ThumbnailSize);
        }

        public static BooruInfo FromReader(BinaryReader Reader)
        {
            return new BooruInfo()
            {
                BooruName = Reader.ReadString(),
                BooruCreator = Reader.ReadString(),
                ThumbnailSize = Reader.ReadUInt16()
            };
        }
    }
}