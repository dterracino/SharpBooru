using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace TEAM_ALPHA.SharpBooru
{
    [Serializable]
    public class Booru:ISerializable
    {
        private Booru() { }
        protected Booru(SerializationInfo info, StreamingContext context)
        {
            Posts = (Dictionary<int, BooruPost>)info.GetValue("posts", typeof(Dictionary<int, BooruPost>));
            Tags = (Dictionary<int, BooruTag>)info.GetValue("tags", typeof(Dictionary<int, BooruTag>));
            Aliases = (Dictionary<string, BooruTag>)info.GetValue("aliases", typeof(Dictionary<string, BooruTag>));
        }

        public Dictionary<int, BooruPost> Posts = new Dictionary<int, BooruPost>();
        public Dictionary<int, BooruTag> Tags = new Dictionary<int,BooruTag>();
        public Dictionary<string, BooruTag> Aliases = new Dictionary<string, BooruTag>();
        
        private string _Folder;
        public string Folder { get { return _Folder; } }

        public static Booru Load(string Folder)
        {
            if (!string.IsNullOrWhiteSpace(Folder))
            {
                if (!Folder.EndsWith("\\"))
                    Folder += "\\";
                if (Directory.Exists(Folder))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    Booru booru = null;
                    if (File.Exists(Folder + "booru"))
                        using (FileStream fs = File.Open(Folder + "booru", FileMode.Open, FileAccess.Read, FileShare.Read))
                            booru = (Booru)formatter.Deserialize(fs);
                    else booru = new Booru();
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
                formatter.Serialize(fs, this);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("posts", Posts);
            info.AddValue("tags", Tags);
            info.AddValue("aliases", Aliases);
        }
    }
}
