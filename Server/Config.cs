using System;
using System.IO;
using System.Xml;
using System.Net.Mail;

namespace TA.SharpBooru
{
    public class Config
    {
        public readonly string User;
        public readonly string Socket;

        public readonly bool EnableMailNotificator;
        public readonly string MailNotificatorServer;
        public readonly ushort MailNotificatorPort;
        public readonly string MailNotificatorUsername;
        public readonly string MailNotificatorPassword;
        public readonly MailAddress MailNotificatorSender;
        public readonly MailAddress MailNotificatorReceiver;

        public Config(string ConfigPath)
        {
            XmlDocument xml = new XmlDocument();
            using (FileStream fs = new FileStream(ConfigPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                xml.Load(fs);

            XmlNode rootNode = xml.SelectSingleNode("/BooruConfig/Server");
            User = rootNode.SelectSingleNode("User").InnerText;
            Socket = rootNode.SelectSingleNode("Socket").InnerText;

            XmlNode mnNode = rootNode.SelectSingleNode("MailNotificator");
            EnableMailNotificator = Convert.ToBoolean(mnNode.Attributes["enable"].Value);
            MailNotificatorServer = mnNode.SelectSingleNode("Server").InnerText;
            MailNotificatorPort = Convert.ToUInt16(mnNode.SelectSingleNode("Port").InnerText);
            MailNotificatorUsername = mnNode.SelectSingleNode("Username").InnerText;
            MailNotificatorPassword = mnNode.SelectSingleNode("Password").InnerText;
            MailNotificatorSender = new MailAddress(mnNode.SelectSingleNode("Sender").InnerText);
            MailNotificatorReceiver = new MailAddress(mnNode.SelectSingleNode("Receiver").InnerText);
        }
    }
}
