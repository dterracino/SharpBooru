using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.IO.Compression;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace TEAM_ALPHA.SharpBooru
{
    [Serializable]
    public class ServerBooru : ISerializable
    {
        private ServerBooru() { }
        protected ServerBooru(SerializationInfo info, StreamingContext context)
        {
            Posts = (List<BooruPost>)info.GetValue("posts", typeof(List<BooruPost>));
            //Tags = (Dictionary<int, BooruTag>)info.GetValue("tags", typeof(Dictionary<int, BooruTag>));
            //Aliases = (Dictionary<string, BooruTag>)info.GetValue("aliases", typeof(Dictionary<string, BooruTag>));
        }

        public List<BooruPost> Posts = new List<BooruPost>();
        //public Dictionary<int, BooruTag> Tags = new Dictionary<int,BooruTag>();
        //public Dictionary<string, BooruTag> Aliases = new Dictionary<string, BooruTag>();
        //public Dictionary<int, BooruTag.TagType> TagTypes

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