using System;
using System.IO;
using System.Data;
using System.Text;

namespace TA.SharpBooru.Server
{
    public class Updater
    {
        private Logger _Logger;
        private string _SetUIDUser;
        private string _BooruPath;
        private string _VersionPath { get { return Path.Combine(_BooruPath, "version"); } }
        private string _DatabasePath { get { return Path.Combine(_BooruPath, "booru.db"); } }

        public Updater(Logger Logger, string BooruPath, string SetUIDUser)
        {
            _Logger = Logger ?? Logger.Null;
            _BooruPath = BooruPath;
            _SetUIDUser = SetUIDUser;
        }

        public void Update()
        {
            int currentVersion = GetVersion();
            int targetVersion = Helper.GetVersionMinor();
            _Logger.LogLine("Updater: Booru database version: {0} - Server version: {1}", currentVersion, targetVersion);
            if (currentVersion < targetVersion)
            {
                _Logger.LogLine("Updater: Update needed, now updating...");
                for (int i = currentVersion; i < targetVersion; i++)
                {
                    _Logger.LogLine("Updater: Updating from version {0} to {1}...", i, i + 1);
                    internalUpdate(i);
                    SaveVersion(i + 1);
                }
                _Logger.LogLine("Updater: Update finished");
            }
            else if (currentVersion > targetVersion)
                throw new NotSupportedException("Database version is higher then the server version");
            else _Logger.LogLine("Updater: No update needed");
        }

        private int GetVersion()
        {
            if (File.Exists(_VersionPath))
            {
                using (FileStream fs = new FileStream(_VersionPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (StreamReader reader = new StreamReader(fs, Encoding.ASCII))
                {
                    string firstLine = reader.ReadLine();
                    return Convert.ToInt32(firstLine);
                }
            }
            else
            {
                SaveVersion(1);
                if (!string.IsNullOrWhiteSpace(_SetUIDUser))
                    ServerHelper.ChOwn(_VersionPath, _SetUIDUser);
                return 1;
            }
        }

        private void SaveVersion(int Version)
        {
            FileMode fileMode = File.Exists(_VersionPath) ? FileMode.Truncate : FileMode.Create;
            using (FileStream fs = new FileStream(_VersionPath, fileMode, FileAccess.Write, FileShare.Read))
            using (StreamWriter writer = new StreamWriter(fs, Encoding.ASCII))
                writer.Write(Version);
        }

        private void internalUpdate(int CurrentVersion)
        {
            switch (CurrentVersion)
            {
                case 50:
                    using (SQLiteWrapper db = new SQLiteWrapper(_DatabasePath))
                    {
                        db.ExecuteNonQuery("ALTER TABLE misc_options RENAME TO options");
                        db.ExecuteNonQuery("CREATE TABLE \"public_keys\" (\"id\" INTEGER PRIMARY KEY NOT NULL, \"username\" TEXT NOT NULL, \"public_key\" TEXT NOT NULL)");
                    }
                    break;

                case 51: RecreateImageHashes(); break;

                case 53:
                    string oldPath = Path.Combine(_BooruPath, "rsa.xml");
                    string newPath = Path.Combine(_BooruPath, "keypair.xml");
                    if (File.Exists(oldPath))
                        File.Move(oldPath, newPath);
                    break;
            }
        }

        private bool _ImageHashesRecreated = false;
        private void RecreateImageHashes()
        {
            if (!_ImageHashesRecreated)
            {
                string imagePath = Path.Combine(_BooruPath, "images", "image");
                using (SQLiteWrapper db = new SQLiteWrapper(_DatabasePath))
                using (DataTable posts = db.ExecuteTable(SQLStatements.GetPosts))
                    foreach (DataRow row in posts.Rows)
                    {
                        ulong id = Convert.ToUInt64(row["id"]);
                        _Logger.LogLine("Recreating image hash of ID {0}...", id);
                        using (BooruImage image = BooruImage.FromFile(imagePath + id))
                            db.ExecuteNonQuery(SQLStatements.UpdateImageHash, Convert.ToBase64String(image.CalculateImageHash()), id);
                    }
                _ImageHashesRecreated = true;
            }
        }
    }
}