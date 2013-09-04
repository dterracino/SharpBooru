using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace TA.SharpBooru.Server
{
    public class Booru
    {
        private static object _LockIO = new object();

        public string Folder;
        public Dictionary<string, string> Configuration = new Dictionary<string, string>();
        public List<BooruUser> Users = new List<BooruUser>();

        public ulong PostIDCounter = 0;
        public ulong TagIDCounter = 0;
        public BooruPostList Posts = new BooruPostList();
        public BooruTagList Tags = new BooruTagList();
        public BooruInfo Info = new BooruInfo();

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
                    //TODO Implement bandwidth limit
                    int read = file.Read(buffer, 0, 1024);
                    if (read > 0)
                        Writer.Write(buffer, 0, read);
                    else break;
                }
            }
        }

        public void ToDiskWriter(BinaryWriter Writer)
        {
            Info.ToWriter(Writer);
            Writer.Write(PostIDCounter);
            Writer.Write(TagIDCounter);
            Writer.Write((uint)Configuration.Count);
            foreach (var configEntry in Configuration)
            {
                Writer.Write(configEntry.Key);
                Writer.Write(configEntry.Value);
            }
            Writer.Write((uint)Users.Count);
            Users.ForEach(x => x.ToWriter(Writer, true));
            Posts.ToDiskWriter(Writer);
            Tags.ToWriter(Writer);
        }

        public static Booru FromDiskReader(BinaryReader Reader)
        {
            Booru booru = new Booru()
            {
                Info = BooruInfo.FromReader(Reader),
                PostIDCounter = Reader.ReadUInt64(),
                TagIDCounter = Reader.ReadUInt64()
            };
            uint configCount = Reader.ReadUInt32();
            for (uint i = 0; i < configCount; i++)
                booru.Configuration.Add(Reader.ReadString(), Reader.ReadString());
            uint userCount = Reader.ReadUInt32();
            for (uint i = 0; i < userCount; i++)
                booru.Users.Add(BooruUser.FromReader(Reader, true));
            booru.Posts = BooruPostList.FromDiskReader(Reader);
            booru.Tags = BooruTagList.FromReader(Reader);
            return booru;
        }

        public void SaveToDisk()
        {
            lock (_LockIO)
            {
                if (File.Exists(Path.Combine(Folder, "booru")))
                {
                    if (File.Exists(Path.Combine(Folder, "booru.bak")))
                        File.Delete(Path.Combine(Folder, "booru.bak"));
                    File.Move(Path.Combine(Folder, "booru"), Path.Combine(Folder, "booru.bak"));
                }
                using (FileStream file = File.Open(Path.Combine(Folder, "booru"), FileMode.Create, FileAccess.Write, FileShare.None))
                using (BinaryWriter writer = new BinaryWriter(file, Encoding.Unicode))
                    ToDiskWriter(writer);
            }
        }

        public void SaveToDisk(string Folder)
        {
            lock (_LockIO)
            {
                this.Folder = Folder;
                SaveToDisk();
            }
        }

        public static Booru ReadFromDisk(string Folder)
        {
            lock (_LockIO)
            {
                using (FileStream file = File.Open(Path.Combine(Folder, "booru"), FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader reader = new BinaryReader(file, Encoding.Unicode))
                {
                    Booru booru = Booru.FromDiskReader(reader);
                    booru.Folder = Folder;
                    return booru;
                }
            }
        }

        public static bool Exists(string Folder) { return File.Exists(Path.Combine(Folder, "booru")); }
    }
}