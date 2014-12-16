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
        public class SocketConfig
        {
            public Socket Socket;
            public EndPoint EndPoint;

            public bool UseTLS = false;
            public string UnixSocketPath = null;
            public FilePermissions UnixSocketPerms = 0;

            public SocketConfig(Socket Socket, EndPoint EndPoint)
            {
                this.Socket = Socket;
                this.EndPoint = EndPoint;
            }
        }

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
                SocketConfigs.Add(ParseSocketConfig(node));
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

        private static SocketConfig ParseSocketConfig(XmlNode node)
        {
            string type = node.Attributes["type"].Value.Trim().ToLower();
            switch (type)
            {
                case "unix":
                    {
                        var socket = new Socket(AddressFamily.Unix, SocketType.Stream, 0);
                        var endPoint = new UnixEndPoint(node.InnerText);
                        string strPerms = node.Attributes["perms"].Value.Trim();
                        return new SocketConfig(socket, endPoint)
                        {
                            UnixSocketPath = node.InnerText,
                            UnixSocketPerms = ParseUnixSocketPerms(strPerms)
                        };
                    }

                case "tcp":
                    {
                        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        var endPoint = Helper.GetIPEndPointFromString(node.InnerText);
                        string strTls = node.Attributes["tls"].Value.Trim();
                        return new SocketConfig(socket, endPoint) { UseTLS = Convert.ToBoolean(strTls) };
                    }

                default: throw new Exception("Unknown node '" + node.Name + "'");
            }
        }

        private static FilePermissions ParseUnixSocketPerms(string str)
        {
            uint perms = ParseSingleOct(str[2], 4, 2, 1);
            perms |= ParseSingleOct(str[1], 32, 16, 8);
            perms |= ParseSingleOct(str[0], 256, 128, 64);
            perms |= 0xC000; //S_IFSOCK;
            return (FilePermissions)perms;
        }

        private static uint ParseSingleOct(char charOct, uint r, uint w, uint x)
        {
            uint perms = 0;
            byte oct = Convert.ToByte(charOct);
            if ((oct & 4) > 0) perms |= r;
            if ((oct & 2) > 0) perms |= w;
            if ((oct & 1) > 0) perms |= x;
            return perms;
        }
    }
}
