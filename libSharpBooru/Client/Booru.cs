using System;
using System.IO;
using System.Net;
using System.Text;
using System.Security;
using System.Net.Sockets;
using System.Net.Security;
using System.Collections.Generic;

namespace TA.SharpBooru.Client
{
    public class Booru
    {
        public static readonly uint ClientVersion = 1;

        private IPEndPoint _EndPoint;
        private string _Username, _Password;

        private TcpClient _Client;
        private SslStream _SSLStream;
        private BinaryReader _Reader;
        private BinaryWriter _Writer;

        public Booru(string Server, ushort Port, string Username, string Password)
        {
            if (string.IsNullOrWhiteSpace(Username))
                throw new ArgumentException("Username");
            else if (string.IsNullOrEmpty(Password))
                throw new ArgumentException("Password");
            IPAddress address = null;
            if (!IPAddress.TryParse(Server, out address))
            {
                IPHostEntry entry = Dns.GetHostEntry(Server);
                List<IPAddress> validIPs = new List<IPAddress>();
                foreach (IPAddress IP in entry.AddressList)
                    if (IP.AddressFamily == AddressFamily.InterNetwork)
                        validIPs.Add(IP);
                if (validIPs.Count < 1)
                    throw new Exception("No IPv4 address");
                else if (validIPs.Count > 1)
                {
                    int randomIndex = (new Random()).Next(0, validIPs.Count);
                    address = validIPs[randomIndex];
                }
                else address = validIPs[0];
            }
            _EndPoint = new IPEndPoint(address, Port);
            _Client = new TcpClient();
            _Username = Username;
            _Password = Password;
        }

        public void Connect()
        {
            if (!_Client.Connected)
            {
                try
                {
                    _Reader.Close();
                    _Writer.Close();
                    _SSLStream.Close();
                    _Client.Close();
                    _Client = new TcpClient();
                }
                catch { }
                _Client.Connect(_EndPoint);
                _SSLStream = new SslStream(_Client.GetStream(), true, delegate { return true; });
                _SSLStream.AuthenticateAsClient("SharpBooruServer");
                _Reader = new BinaryReader(_SSLStream, Encoding.Unicode);
                _Writer = new BinaryWriter(_SSLStream, Encoding.Unicode);
                _Writer.Write(ClientVersion);
                if (_Reader.ReadBoolean())
                    throw new ProtocolViolationException("Server version mismatch");
                _Writer.Write(_Username);
                _Writer.Write(_Password);
                if (_Reader.ReadBoolean())
                    throw new SecurityException("Login failed");
            }
        }

        public TOut Communicate<TOut, TIn>(BooruProtocol.Command Command, TIn Payload) { return (TOut)Communicate(Command, Payload); }
        public object Communicate(BooruProtocol.Command Command, object Payload)
        {
            Connect();
            _Writer.Write((byte)Command);
            switch (Command)
            {
                case BooruProtocol.Command.GetPost:
                    {
                        _Writer.Write((ulong)Payload);
                        var errorCode = (BooruProtocol.ErrorCode)_Reader.ReadByte();
                        if (errorCode == BooruProtocol.ErrorCode.Success)
                            return BooruPost.FromReader(_Reader);
                        else return null;
                    }
                case BooruProtocol.Command.GetImage:
                    {
                        _Writer.Write((ulong)Payload);
                        var errorCode = (BooruProtocol.ErrorCode)_Reader.ReadByte();
                        if (errorCode == BooruProtocol.ErrorCode.Success)
                        {
                            int byteCount = (int)_Reader.ReadUInt32();
                            return _Reader.ReadBytes(byteCount);
                        }
                        else return null;
                    }
                case BooruProtocol.Command.SaveBooru:
                    _Reader.ReadByte();
                    return true;
                case BooruProtocol.Command.Search:
                    {
                        _Writer.Write((string)Payload);
                        var errorCode = (BooruProtocol.ErrorCode)_Reader.ReadByte();
                        if (errorCode == BooruProtocol.ErrorCode.Success)
                        {
                            uint count = _Reader.ReadUInt32();
                            List<ulong> IDs = new List<ulong>();
                            for (uint i = 0; i < count; i++)
                                IDs.Add(_Reader.ReadUInt64());
                            return IDs;
                        }
                        else return null;
                    }
                //case BooruProtocol.Command.AddPost: //TODO
                //case BooruProtocol.Command.EditPost: _Server.Booru.EditPost(_Reader); break;
                //case BooruProtocol.Command.RemovePost: _Server.Booru.RemovePost(_Reader); break;
                //case BooruProtocol.Command.EditImage: _Server.Booru.EditImage(_Reader, _Writer); break;
                //case BooruProtocol.Command.EditTag: _Server.Booru.EditTag(_Reader, _Writer); break;
                //case BooruProtocol.Command.RemoveTag: _Server.Booru.RemoveTag(_Reader); break;
                default: throw new NotImplementedException();
            }
        }
    }
}