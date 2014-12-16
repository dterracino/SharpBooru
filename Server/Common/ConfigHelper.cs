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
    public class SocketConfig
    {
        public Socket Socket;
        public EndPoint EndPoint;
        public bool UseTLS = false;

        public string UnixSocketPath = null;
        public FilePermissions? UnixSocketPerms = null;

        public SocketConfig(Socket Socket, EndPoint EndPoint)
        {
            this.Socket = Socket;
            this.EndPoint = EndPoint;
        }
    }

    public static class ConfigHelper
    {
        public static SocketConfig ParseSocketConfig(XmlNode Node)
        {
            string type = Node.Attributes["type"].Value.Trim().ToLower();
            switch (type)
            {
                case "unix":
                    {
                        var socket = new Socket(AddressFamily.Unix, SocketType.Stream, 0);
                        var endPoint = new UnixEndPoint(Node.InnerText);
                        XmlAttribute permsAttribute = Node.Attributes["perms"];
                        SocketConfig sockConf = new SocketConfig(socket, endPoint) { UnixSocketPath = Node.InnerText };
                        if (permsAttribute != null)
                            sockConf.UnixSocketPerms = ParseUnixSocketPerms(permsAttribute.Value.Trim());
                        return sockConf;
                    }

                case "tcp":
                    {
                        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        var endPoint = Helper.GetIPEndPointFromString(Node.InnerText);
                        string strTls = Node.Attributes["tls"].Value.Trim();
                        return new SocketConfig(socket, endPoint) { UseTLS = Convert.ToBoolean(strTls) };
                    }

                default: throw new Exception("Unknown node '" + Node.Name + "'");
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
