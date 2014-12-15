using System.Net;
using System.Net.Sockets;
using Mono.Unix;

namespace TA.SharpBooru
{
    public static class SocketParser
    {
        public class ParsedSocket
        {
            public Socket Socket;
            public EndPoint EndPoint;
            public string UnixSocketPath;

            public ParsedSocket(Socket Socket, EndPoint EndPoint, string UnixSocketPath = null)
            {
                this.Socket = Socket;
                this.EndPoint = EndPoint;
                this.UnixSocketPath = UnixSocketPath;
            }
        }

        public static ParsedSocket Parse(string SocketString)
        {
            if (SocketString.StartsWith("unix://"))
                return ParseUnix(SocketString.Substring(6));
            else if (SocketString.StartsWith("tcp://"))
                return ParseTcp(SocketString.Substring(5));
            else return null;
        }

        private static ParsedSocket ParseUnix(string str)
        {
            Socket socket = new Socket(AddressFamily.Unix, SocketType.Stream, 0);
            UnixEndPoint endPoint = new UnixEndPoint(str);
            return new ParsedSocket(socket, endPoint, str);
        }

        private static ParsedSocket ParseTcp(string str)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = Helper.GetIPEndPointFromString(str);
            return new ParsedSocket(socket, endPoint);
        }
    }
}
