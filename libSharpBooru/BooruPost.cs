using System;
using System.IO;
using System.Collections.Generic;

namespace TA.SharpBooru
{
    public class BooruPost
    {
        public bool Private = false;
        public string Owner = string.Empty;
        public byte Rating = 0;
        public string Source = string.Empty;
        public string Comment = string.Empty;
        public uint Width = 0;
        public uint Height = 0;
        public BooruTagList Tags = new BooruTagList();
        //TODO Thumbnail
        public ulong ID = 0;
        public DateTime CreationDate = DateTime.Now;
        public ulong ViewCount = 0;
        public ulong EditCount = 0;
        public long Score = 0;

        public static bool operator ==(BooruPost Post1, BooruPost Post2)
        {
            if ((object)Post1 == null && (object)Post2 == null)
                return true;
            else if ((object)Post1 == null || (object)Post2 == null)
                return false;
            else return Post1.ID == Post2.ID;
        }

        public static bool operator !=(BooruPost Post1, BooruPost Post2) { return !(Post1 == Post2); }

        public override bool Equals(object obj) { return this.ID == (obj as BooruPost).ID; }

        public override int GetHashCode() { return ID.GetHashCode(); }

        public void ToWriter(BinaryWriter Writer)
        {
            Writer.Write(ID);
            Writer.Write(Private);
            Writer.Write(Owner);
            Writer.Write(Rating);
            Writer.Write(Source);
            Writer.Write(Comment);
            Writer.Write(Width);
            Writer.Write(Height);
            Writer.Write(Helper.DateTimeToUnixTime(CreationDate));
            Writer.Write(ViewCount);
            Writer.Write(EditCount);
            Writer.Write(Score);
            Tags.ToWriter(Writer);
        }

        public static BooruPost FromReader(BinaryReader Reader)
        {
            return new BooruPost()
            {
                ID = Reader.ReadUInt64(),
                Private = Reader.ReadBoolean(),
                Owner = Reader.ReadString(),
                Rating = Reader.ReadByte(),
                Source = Reader.ReadString(),
                Comment = Reader.ReadString(),
                Width = Reader.ReadUInt32(),
                Height = Reader.ReadUInt32(),
                CreationDate = Helper.UnixTimeToDateTime(Reader.ReadUInt32()),
                ViewCount = Reader.ReadUInt64(),
                EditCount = Reader.ReadUInt64(),
                Score = Reader.ReadInt64(),
                Tags = BooruTagList.FromReader(Reader)
            };
        }
    }

    public class BooruPostList : List<BooruPost>
    {
        public BooruPost this[ulong ID]
        {
            get
            {
                foreach (BooruPost post in this)
                    if (post.ID == ID)
                        return post;
                return null;
            }
        }

        public bool Contains(ulong ID) { return this[ID] != null; }

        public int Remove(ulong ID) { return this.RemoveAll(x => { return x.ID == ID; }); }

        public void ToWriter(BinaryWriter Writer)
        {
            Writer.Write((uint)this.Count);
            this.ForEach(x => x.ToWriter(Writer));
        }

        public static BooruPostList FromReader(BinaryReader Reader)
        {
            uint count = Reader.ReadUInt32();
            BooruPostList bTagList = new BooruPostList();
            for (uint i = 0; i < count; i++)
                bTagList.Add(BooruPost.FromReader(Reader));
            return bTagList;
        }
    }
}