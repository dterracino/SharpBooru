using System;
using System.IO;
using System.Data;
using System.Drawing;
using System.Collections.Generic;

namespace TA.SharpBooru
{
    public class BooruPost : BooruResource, ICloneable, IDisposable
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
        public byte[] ImageHash = new byte[0];
        public BooruTagList Tags = new BooruTagList();

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

        public override void ToWriter(ReaderWriter Writer)
        {
            Writer.Write(ID);
            Writer.Write(User, true);
            Writer.Write(Private);
            Writer.Write(Source, true);
            Writer.Write(Description, true);
            Writer.Write(Rating);
            Writer.Write(Width);
            Writer.Write(Height);
            Writer.Write(Helper.DateTimeToUnixTime(CreationDate));
            Writer.Write(ViewCount);
            Writer.Write(EditCount);
            Writer.Write(Score);
            Writer.Write(ImageHash, true);
            Tags.ToWriter(Writer);
            Writer.Write(Thumbnail != null);
            if (Thumbnail != null)
                Thumbnail.ToWriter(Writer);
            Writer.Write(Image != null);
            if (Image != null)
                Image.ToWriter(Writer);
        }

        public static BooruPost FromReader(ReaderWriter Reader)
        {
            BooruPost post = new BooruPost()
            {
                ID = Reader.ReadULong(),
                User = Reader.ReadString(),
                Private = Reader.ReadBool(),
                Source = Reader.ReadString(),
                Description = Reader.ReadString(),
                Rating = Reader.ReadByte(),
                Width = Reader.ReadUInt(),
                Height = Reader.ReadUInt(),
                CreationDate = Helper.UnixTimeToDateTime(Reader.ReadUInt()),
                ViewCount = Reader.ReadULong(),
                EditCount = Reader.ReadULong(),
                Score = Reader.ReadLong(),
                ImageHash = Reader.ReadBytes()
            };
            post.Tags = BooruTagList.FromReader(Reader);
            if (Reader.ReadBool())
                post.Thumbnail = BooruImage.FromReader(Reader);
            if (Reader.ReadBool())
                post.Image = BooruImage.FromReader(Reader);
            return post;
        }

        /// <summary>Creates a BooruPost from a DataRow. DOES NOT include Tags, Thumb and Image</summary>
        public static BooruPost FromRow(DataRow Row)
        {
            if (Row != null)
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
                    ImageHash = Convert.FromBase64String(Convert.ToString(Row["hash"])),
                };
            else return null;
        }

        public Dictionary<string, object> ToDictionary(bool IncludeID)
        {
            var dict = new Dictionary<string, object>()
            {
                { "user", User },
                { "private", Private.ToString() },
                { "source", Source },
                { "description", Description },
                { "rating", Rating },
                { "width", Width },
                { "height", Height },
                { "creationdate", Helper.DateTimeToUnixTime(CreationDate) },
                { "viewcount", ViewCount },
                { "editcount", EditCount },
                { "score", Score },
                { "hash", Convert.ToBase64String(ImageHash) }
            };
            if (IncludeID)
                dict.Add("id", ID);
            return dict;
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

        public void Dispose()
        {
            if (Image != null)
            {
                Image.Dispose();
                Image = null;
            }
            if (Thumbnail != null)
            {
                Thumbnail.Dispose();
                Thumbnail = null;
            }
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

        public void ToClientWriter(ReaderWriter Writer)
        {
            Writer.Write((uint)this.Count);
            this.ForEach(x => x.ToWriter(Writer));
        }

        public static BooruPostList FromClientReader(ReaderWriter Reader)
        {
            uint count = Reader.ReadUInt();
            BooruPostList bTagList = new BooruPostList();
            for (uint i = 0; i < count; i++)
                bTagList.Add(BooruPost.FromReader(Reader));
            return bTagList;
        }

        public static BooruPostList FromTable(DataTable Table)
        {
            BooruPostList bPostList = new BooruPostList();
            foreach (DataRow row in Table.Rows)
                bPostList.Add(BooruPost.FromRow(row));
            return bPostList;
        }

        public void Refresh(BooruPost Post)
        {
            //TODO Improve (Don't remove and add, but refresh!, maybe not needed since Post is a class)
            Remove(Post.ID);
            Add(Post);
        }
    }
}