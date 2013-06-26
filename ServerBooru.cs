using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.IO.Compression;
using System.Collections.Generic;

namespace TA.SharpBooru.Server
{
    public class ServerBooru
    {

        public List<BooruPost> Posts = new List<BooruPost>();

        private string _Folder;
        public string Folder { get { return _Folder; } }

        public static ServerBooru Load(string Folder)
        {
            if (!string.IsNullOrWhiteSpace(Folder))
            {
                if (!Folder.EndsWith("\\"))
                    Folder += "\\";
                if (Directory.Exists(Folder))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    ServerBooru booru = null;
                    if (File.Exists(Folder + "booru"))
                        using (FileStream fs = File.Open(Folder + "booru", FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (GZipStream gzstream = new GZipStream(fs, CompressionMode.Decompress))
                            booru = (ServerBooru)formatter.Deserialize(gzstream);
                    else booru = new ServerBooru();
                    booru._Folder = Folder;
                    return booru;
                }
                else throw new DirectoryNotFoundException();
            }
            else throw new ArgumentException();
        }

        public void Save()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = File.Open(_Folder + "booru", FileMode.Create, FileAccess.Write, FileShare.Read))
            using (GZipStream gzstream = new GZipStream(fs, CompressionMode.Compress))
                formatter.Serialize(gzstream, this);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("posts", Posts);
            //info.AddValue("tags", Tags);
            //info.AddValue("aliases", Aliases);
        }

        public void Add(BooruPost Post) { Posts.Add(Post); }
        public void Remove(BooruPost Post) { Posts.Remove(Post); }
        public BooruPost this[int Index] { get { return Posts[Index]; } set { Posts[Index] = value; } }
    }
}