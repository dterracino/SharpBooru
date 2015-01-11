using System;
using System.IO;
using System.Xml;

namespace TA.SharpBooru
{
    public class Config
    {
        public readonly SocketConfig SocketConfig;
        public readonly bool CheckCertificate;
        public readonly string Username;
        public readonly string Password;

        public Config(SocketConfig SocketConfig, bool CheckCertificate, string Username, string Password)
        {
            this.SocketConfig = SocketConfig;
            this.CheckCertificate = CheckCertificate;
            this.Username = Username;
            this.Password = Password;
        }

        public static Config TryLoad()
        {
            string[] paths = new string[4];
            paths[0] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".config", "booru", "client.xml");
            paths[1] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "booru.xml");
            paths[2] = "/etc/booru/client.xml";
            paths[3] = "/srv/booru/config.xml";

            for (int i = 0; i < paths.Length; i++)
                if (File.Exists(paths[i]))
                {
                    XmlDocument xml = new XmlDocument();
                    using (FileStream fs = new FileStream(paths[i], FileMode.Open, FileAccess.Read, FileShare.Read))
                        xml.Load(fs);

                    XmlNode rootNode = xml.SelectSingleNode("/BooruConfig/Client");

                    bool checkCert = Convert.ToBoolean(rootNode.SelectSingleNode("CheckCertificate").InnerText);
                    var sockConf = ConfigHelper.ParseSocketConfig(rootNode.SelectSingleNode("Socket"));
                    string username = rootNode.SelectSingleNode("Username").InnerText;
                    string password = rootNode.SelectSingleNode("Password").InnerText;

                    return new Config(sockConf, checkCert, username, password);
                }

            return null;
        }
    }
}
