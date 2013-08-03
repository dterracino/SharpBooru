using System;
using System.IO;
using System.Collections.Generic;

namespace TA.SharpBooru.Client
{
    public class BooruPost : ICloneable
    {
        public bool Private = false;
        public string Owner = string.Empty;
        public byte Rating = 0;
        public string Source = string.Empty;
        public string Comment = string.Empty;
        public uint Width = 0;
        public uint Height = 0;
        public BooruTagList Tags = new BooruTagList();

        public ulong ID = 0;
        public DateTime CreationDate = DateTime.Now;
        public ulong ViewCount = 0;
        public ulong EditCount = 0;
        public long Score = 0;

        public BooruImage Image = null;
        public BooruImage Thumbnail = null;

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

        public void ToServerWriter(BinaryWriter Writer)
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
            Writer.Write((uint)Tags.Count);
            Tags.ForEach(x => Writer.Write(x.Tag));
        }

        public static BooruPost FromServerReader(BinaryReader Reader)
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

        public object Clone()
        {
            BooruPost post = MemberwiseClone() as BooruPost;
            if (Image != null)
                post.Image = Image.Clone() as BooruImage;
            if (Thumbnail != null)
                post.Thumbnail = Thumbnail.Clone() as BooruImage;
            post.Tags = Tags.Clone() as BooruTagList;
            return post;
        }
    }

    public class BooruPostList : List<BooruPost>, ICloneable
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

        public new bool Contains(BooruPost Post) { return Contains(Post.ID); }
        public bool Contains(ulong ID) { return this[ID] != null; }

        public int Remove(ulong ID) { return this.RemoveAll(x => { return x.ID == ID; }); }

        public void ToServerWriter(BinaryWriter Writer)
        {
            Writer.Write((uint)this.Count);
            this.ForEach(x => x.ToServerWriter(Writer));
        }

        public static BooruPostList FromServerReader(BinaryReader Reader)
        {
            uint count = Reader.ReadUInt32();
            BooruPostList bTagList = new BooruPostList();
            for (uint i = 0; i < count; i++)
                bTagList.Add(BooruPost.FromServerReader(Reader));
            return bTagList;
        }

        public object Clone()
        {
            BooruPostList cList = new BooruPostList();
            this.ForEach(x => cList.Add(x.Clone() as BooruPost));
            return cList;
        }
    }
}