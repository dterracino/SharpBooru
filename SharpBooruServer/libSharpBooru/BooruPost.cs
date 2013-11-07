using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

namespace TA.SharpBooru.Server
{
    public class BooruPost
    {
        public ulong ID = 0;
        public string User = string.Empty;
        public bool Private = false;
        public string Source = string.Empty;
        public string Description = string.Empty;
        public byte Rating = 0;
        public uint Width = 0;
        public uint Height = 0;
        public DateTime CreationDate = DateTime.Now;
        public ulong ViewCount = 0;
        public ulong EditCount = 0;
        public long Score = 0;
        public ulong ImageHash = 0;
        public BooruTagList Tags = null;
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

        public void ToWriter(BinaryWriter Writer)
        {
            Writer.Write(ID);
            Writer.Write(User);
            Writer.Write(Private);
            Writer.Write(Source);
            Writer.Write(Description);
            Writer.Write(Rating);
            Writer.Write(Width);
            Writer.Write(Height);
            Writer.Write(Helper.DateTimeToUnixTime(CreationDate));
            Writer.Write(ViewCount);
            Writer.Write(EditCount);
            Writer.Write(Score);
            Writer.Write(ImageHash);
            Tags.ToWriter(Writer);
        }

        public static BooruPost FromReader(BinaryReader Reader)
        {
            BooruPost post = new BooruPost()
            {
                ID = Reader.ReadUInt64(),
                User = Reader.ReadString(),
                Private = Reader.ReadBoolean(),
                Source = Reader.ReadString(),
                Description = Reader.ReadString(),
                Rating = Reader.ReadByte(),
                Width = Reader.ReadUInt32(),
                Height = Reader.ReadUInt32(),
                CreationDate = Helper.UnixTimeToDateTime(Reader.ReadUInt32()),
                ViewCount = Reader.ReadUInt64(),
                EditCount = Reader.ReadUInt64(),
                Score = Reader.ReadInt64(),
                ImageHash = Reader.ReadUInt64()
            };
            uint count = Reader.ReadUInt32();
            for (uint i = 0; i < count; i++)
                post.Tags.Add(new BooruTag(Reader.ReadString(), "Undefined", "Undefined tags", Color.Black));
            return post;
        }

        /// <summary>Creates a BooruPost from a DataRow. DOES NOT include Tags, Thumb and Image</summary>
        public static BooruPost FromRow(DataRow Row)
        {
            return new BooruPost()
            {
                ID = Convert.ToUInt64(Row["id"]),
                User = Convert.ToString(Row["user"]),
                Private = Convert.ToBoolean(Row["private"]),
                Source = Convert.ToString(Row["source"]),
                Description = Convert.ToString(Row["description"]),
                Rating = Convert.ToByte(Row["rating"]),
                Width = Convert.ToUInt32(Row["width"]),
                Height = Convert.ToUInt32(Row["height"]),
                CreationDate = Helper.UnixTimeToDateTime(Convert.ToUInt32(Row["creationdate"])),
                ViewCount = Convert.ToUInt64(Row["viewcount"]),
                EditCount = Convert.ToUInt64(Row["editcount"]),
                Score = Convert.ToInt64(Row["score"]),
                ImageHash = Convert.ToUInt64(Row["hash"]),
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

        public new bool Contains(BooruPost Post) { return Contains(Post.ID); }
        public bool Contains(ulong ID) { return this[ID] != null; }

        public int Remove(ulong ID) { return this.RemoveAll(x => { return x.ID == ID; }); }

        public void ToClientWriter(BinaryWriter Writer, Booru Booru)
        {
            Writer.Write((uint)this.Count);
            this.ForEach(x => x.ToClientWriter(Writer, Booru));
        }

        public static BooruPostList FromClientReader(BinaryReader Reader, Booru Booru)
        {
            uint count = Reader.ReadUInt32();
            BooruPostList bTagList = new BooruPostList();
            for (uint i = 0; i < count; i++)
                bTagList.Add(BooruPost.FromClientReader(Reader, Booru));
            return bTagList;
        }

        public void Refresh(BooruPost Post)
        {
            //TODO Improve (Don't remove and add, but refresh!, maybe not needed since Post is a class)
            Remove(Post.ID);
            Add(Post);
        }
    }
}
