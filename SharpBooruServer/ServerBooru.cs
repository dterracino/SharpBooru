using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Data.SQLite;

namespace TA.SharpBooru.Server
{
    public class ServerBooru
    {
        private SQLiteWrapper _DB;
        private string _Folder;

        public SQLiteWrapper DB { get { return _DB; } }
        //public string Folder { get { return _Folder; } }

        public string ImageFolder { get { return Path.Combine(_Folder, "images"); } }
        public string ThumbFolder { get { return Path.Combine(_Folder, "thumbs"); } }
        //public string AvatarFolder { get { return Path.Combine(_Folder, "avatars"); } }

        public ServerBooru(string Folder)
        {
            string dbPath = Path.Combine(Folder, "booru.db");
            if (File.Exists(dbPath))
                if (Directory.Exists(Path.Combine(Folder, "images")))
                    if (Directory.Exists(Path.Combine(Folder, "thumbs")))
                    //if (Directory.Exists(Path.Combine(Folder, "avatars")))
                    {
                        _Folder = Folder;
                        _DB = new SQLiteWrapper(dbPath);
                        _DB.Connect();
                        return;
                    }
            throw new Exception("No valid booru directory");
        }

        public void ReadThumb(BinaryWriter Writer, ulong ID) { ReadFile(Writer, Path.Combine("thumbs", "thumb" + ID)); }
        public void ReadImage(BinaryWriter Writer, ulong ID) { ReadFile(Writer, Path.Combine("images", "image" + ID)); }

        private void ReadFile(BinaryWriter Writer, string Name)
        {
            using (FileStream file = File.Open(Path.Combine(_Folder, Name), FileMode.Open, FileAccess.Read, FileShare.Read))
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
    }
}