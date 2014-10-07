using System;
using System.Data;
using System.Collections.Generic;

namespace TA.SharpBooru
{
    public class BooruInfo : BooruResource
    {
        public string BooruName { get; set; }
        public string BooruCreator { get; set; }
        public ushort ThumbnailSize { get; set; }
        public byte ThumbnailQuality { get; set; }
        public byte DefaultRating { get; set; }
        public byte DefaultTagType { get; set; }

        public override void ToWriter(ReaderWriter Writer)
        {
            Writer.Write(BooruName, true);
            Writer.Write(BooruCreator, true);
            Writer.Write(ThumbnailSize);
            Writer.Write(ThumbnailQuality);
            Writer.Write(DefaultRating);
            Writer.Write(DefaultTagType);
        }

        public static BooruInfo FromReader(ReaderWriter Reader)
        {
            return new BooruInfo()
            {
                BooruName = Reader.ReadString(),
                BooruCreator = Reader.ReadString(),
                ThumbnailSize = Reader.ReadUShort(),
                ThumbnailQuality = Reader.ReadByte(),
                DefaultRating = Reader.ReadByte(),
                DefaultTagType = Reader.ReadByte()
            };
        }

        public static BooruInfo FromTable(DataTable Table)
        {
            var options = new Dictionary<string, object>();
            foreach (DataRow optionRow in Table.Rows)
                options.Add(Convert.ToString(optionRow["key"]), optionRow["value"]);
            return new BooruInfo()
            {
                BooruName = Convert.ToString(options["BooruName"]),
                BooruCreator = Convert.ToString(options["BooruCreator"]),
                ThumbnailSize = Convert.ToUInt16(options["ThumbnailSize"]),
                ThumbnailQuality = Convert.ToByte(options["ThumbnailQuality"]),
                DefaultRating = Convert.ToByte(options["DefaultRating"]),
                DefaultTagType = Convert.ToByte(options["DefaultTagType"])
            };
        }
    }
}
