using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace TA.SharpBooru.Server
{
    public class Booru
    {
        public string Folder;
        public Dictionary<string, string> Configuration = new Dictionary<string, string>();
        public List<BooruUser> Users = new List<BooruUser>();

        public ulong PostIDCounter = 0;
        public ulong TagIDCounter = 0;
        public BooruPostList Posts = new BooruPostList();

        public ulong GetNextPostID() { return PostIDCounter++; }

        public ulong GetNextTagID() { return TagIDCounter++; }

        //Name
        //Creator
        //Description
        //DefaultTagType
        public T GetConfigEntry<T>(string Key)
        {
            string entry = Configuration[Key];
            return (T)Convert.ChangeType(entry, typeof(T));
        }

        public void ReadFile(BinaryWriter Writer, string Name)
        {
            using (FileStream file = File.Open(Path.Combine(Folder, Name), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                uint length = (uint)file.Length;
                Writer.Write(length);
                byte[] buffer = new byte[1024];
                while (true)
                {
                    int read = file.Read(buffer, 0, 1024);
                    if (read > 0)
                        Writer.Write(buffer, 0, read);
                    else break;
                }
            }
        }

        /*

        private BooruTag GetTag(string Tag)
        {
            Tag  = Tag.Trim().ToLower();
            foreach (BooruTag tag in Tags)
                if (tag.Tag == Tag)
                    return tag;
            return new BooruTag(Tag);
        }

        private BooruPost CreateBooruPost(BinaryReader Reader)
        {
            BooruPost post = new BooruPost()
            {
                Private = Reader.ReadBoolean(),
                Rating = Reader.ReadByte(),
                Source = Reader.ReadString(),
                Comment = Reader.ReadString(),
                Width = Reader.ReadUInt32(),
                Height = Reader.ReadUInt32()
            };
            uint tagCount = Reader.ReadUInt32();
            for (int i = 0; i < tagCount; i++)
            {
                string sTag = Reader.ReadString();
                BooruTag bTag = GetTag(sTag);
                post.Tags.Add(bTag.ID);
            }
            return post;
        }

        public void AddPost(BinaryReader Reader)
        {
            BooruPost post = CreateBooruPost(Reader);
            lock (this)
            {
                post.ID = PostIDCounter;
                PostIDCounter++;
            }
            WriteFile(Reader, "thumb" + post.ID);
            WriteFile(Reader, "image" + post.ID);
            Posts.Add(post);
        }

        public void EditPost(BinaryReader Reader)
        {
            ulong postID = Reader.ReadUInt64();
            BooruPost post = CreateBooruPost(Reader);
            post.ID = postID;
            Posts.Remove(postID);
            Posts.Add(post);
        }

        public void RemovePost(BinaryReader Reader)
        {
            ulong postID = Reader.ReadUInt64();
            Posts.Remove(postID);
        }

        public void GetPost(BinaryReader Reader, BinaryWriter Writer)
        {
            ulong postID = Reader.ReadUInt64();
            BooruPost post = Posts[postID];
            Writer.Write(post.ID);
            Writer.Write(post.Private);
            Writer.Write(post.Rating);
            Writer.Write(post.Source);
            Writer.Write(post.Comment);
            Writer.Write(post.Width);
            Writer.Write(post.Height);
            Writer.Write((post.CreationDate - new DateTime(1970, 1, 1).ToUniversalTime()).TotalSeconds);
            Writer.Write(post.ViewCount);
            Writer.Write(post.EditCount);
            Writer.Write(post.Score);
            Writer.Write((uint)post.Tags.Count);
            for (int i = 0; i < post.Tags.Count; i++)
            {
                BooruTag tag = Tags[post.Tags[i]];
                Writer.Write(tag.Tag);
            }
        }

        public void GetImage(BinaryReader Reader, BinaryWriter Writer)
        {
            ulong postID = Reader.ReadUInt64();
            ReadFile(Writer, "image" + postID);
        }

        public void EditImage(BinaryReader Reader, BinaryWriter Writer)
        {
            ulong postID = Reader.ReadUInt64();
            WriteFile(Reader, "image" + postID);
        }

        public void EditTag(BinaryReader Reader, BinaryWriter Writer)
        {
            ulong tagID = Reader.ReadUInt64();
            BooruTag tag = new BooruTag(Reader.ReadString())
            {
                ID = tagID,
                Type = Reader.ReadString(),
                Description = Reader.ReadString(),
                Color = Reader.ReadInt32()
            };
            Tags.Remove(tagID);
            Tags.Add(tag);
        }

        public void RemoveTag(BinaryReader Reader)
        {
            ulong tagID = Reader.ReadUInt64();
            Tags.Remove(tagID);
            foreach (BooruPost post in Posts)
                post.Tags.RemoveAll(x => { return x == tagID; }); //TODO Optimize
        }
        
        */

        public void ToWriter(BinaryWriter Writer)
        {
            Writer.Write(PostIDCounter);
            Writer.Write(TagIDCounter);
            Writer.Write((uint)Configuration.Count);
            foreach (var configEntry in Configuration)
            {
                Writer.Write(configEntry.Key);
                Writer.Write(configEntry.Value);
            }
            Writer.Write((uint)Users.Count);
            Users.ForEach(x => x.ToWriter(Writer));
            Posts.ToWriter(Writer);
        }

        public static Booru FromReader(BinaryReader Reader)
        {
            Booru booru = new Booru()
            {
                PostIDCounter = Reader.ReadUInt64(),
                TagIDCounter = Reader.ReadUInt64()
            };
            uint configCount = Reader.ReadUInt32();
            for (uint i = 0; i < configCount; i++)
                booru.Configuration.Add(Reader.ReadString(), Reader.ReadString());
            uint userCount = Reader.ReadUInt32();
            for (uint i = 0; i < userCount; i++)
                booru.Users.Add(BooruUser.FromReader(Reader));
            booru.Posts = BooruPostList.FromReader(Reader);
            return booru;
        }

        public void SaveToDisk()
        {
            if (File.Exists(Path.Combine(Folder, "booru")))
            {
                if (File.Exists(Path.Combine(Folder, "booru.bak")))
                    File.Delete(Path.Combine(Folder, "booru.bak"));
                File.Move(Path.Combine(Folder, "booru"), Path.Combine(Folder, "booru.bak"));
            }
            using (FileStream file = File.Open(Path.Combine(Folder, "booru"), FileMode.Create, FileAccess.Write, FileShare.None))
            using (BinaryWriter writer = new BinaryWriter(file, Encoding.Unicode))
                ToWriter(writer);
        }

        public void SaveToDisk(string Folder)
        {
            this.Folder = Folder;
            SaveToDisk();
        }

        public static Booru ReadFromDisk(string Folder)
        {
            using (FileStream file = File.Open(Path.Combine(Folder, "booru"), FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader reader = new BinaryReader(file, Encoding.Unicode))
            {
                Booru booru = Booru.FromReader(reader);
                booru.Folder = Folder;
                return booru;
            }
        }

        public static bool Exists(string Folder) { return File.Exists(Path.Combine(Folder, "booru")); }
    }
}