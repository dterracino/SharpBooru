using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Data.SQLite;

namespace TA.SharpBooru.Server
{
    public class Booru
    {
        private SQLiteWrapper _DB;
        private string _Folder;

        public SQLiteWrapper DB { get { return _DB; } }
        public string Folder { get { return _Folder; } }

        public Booru(string Folder)
        {
            string dbPath = Path.Combine(Folder, "booru.db");
            if (File.Exists(dbPath))
                if (Directory.Exists(Path.Combine(Folder, "images")))
                    if (Directory.Exists(Path.Combine(Folder, "thumbs")))
                    {
                        _Folder = Folder;
                        _DB = new SQLiteWrapper(dbPath);
                        _DB.Connect();
                    }
            throw new Exception("No valid booru directory");
        }

        public void ReadThumb(BinaryWriter Writer, ulong ID) { ReadFile(Writer, Path.Combine("thumbs", "thumb" + ID)); }
        public void ReadImage(BinaryWriter Writer, ulong ID) { ReadFile(Writer, Path.Combine("images", "image" + ID)); }

        private void ReadFile(BinaryWriter Writer, string Name)
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
    }
}