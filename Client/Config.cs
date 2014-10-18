using System;
using System.IO;
using System.Xml;

namespace TA.SharpBooru
{
    public class Config
    {
        public readonly string Socket;
        public readonly string Username;
        public readonly string Password;

        public Config(string Socket, string Username, string Password)
        {
            this.Socket = Socket;
            this.Username = Username;
            this.Password = Password;
        }

        public static Config TryLoad()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            path = Path.Combine(path, ".config", "booru", "client.conf");

            if (File.Exists(path))
            {
                XmlDocument xml = new XmlDocument();
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        xml.Load(fs);

                XmlNode rootNode = xml.SelectSingleNode("/BooruClientConfig");
                string socket = rootNode.SelectSingleNode("Socket").InnerText;
                string username = rootNode.SelectSingleNode("Username").InnerText;
                string password = rootNode.SelectSingleNode("Password").InnerText;

                return new Config(socket, username, password);
            }

            return null;
        }
    }
}
