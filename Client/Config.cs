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
            string[] paths = new string[4];
            paths[0] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".config", "booru", "client.conf");
            paths[1] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "booru.conf");
            paths[2] = "/etc/booru/client.xml";
            paths[3] = "/srv/booru/config.xml";

            for (int i = 0; i < paths.Length; i++)
                if (File.Exists(paths[i]))
                {
                    XmlDocument xml = new XmlDocument();
                    using (FileStream fs = new FileStream(paths[i], FileMode.Open, FileAccess.Read, FileShare.Read))
                        xml.Load(fs);

                    XmlNode rootNode = xml.SelectSingleNode("/BooruConfig/Client");
                    string socket = rootNode.SelectSingleNode("Socket").InnerText;
                    string username = rootNode.SelectSingleNode("Username").InnerText;
                    string password = rootNode.SelectSingleNode("Password").InnerText;
                    return new Config(socket, username, password);
                }

            return null;
        }
    }
}
