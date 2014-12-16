using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Net.Mail;
using System.Net.Sockets;
using System.Collections.Generic;
using Mono.Unix;
using Mono.Unix.Native;

namespace TA.SharpBooru
{
    public class Config
    {
        public readonly string User;
        public readonly string Certificate;

        public readonly List<SocketConfig> SocketConfigs;
        public readonly bool CertificateNeeded = false;

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
            Certificate = rootNode.SelectSingleNode("Certificate").InnerText;

            XmlNode sockNode = rootNode.SelectSingleNode("Sockets");
            SocketConfigs = new List<SocketConfig>(2);
            foreach (XmlNode node in sockNode.ChildNodes)
                SocketConfigs.Add(ConfigHelper.ParseSocketConfig(node));
            foreach (var sockConf in SocketConfigs)
                if (sockConf.UseTLS)
                {
                    CertificateNeeded = true;
                    break;
                }

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
