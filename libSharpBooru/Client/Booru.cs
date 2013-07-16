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
    public class Booru : IDisposable
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
                Disconnect();
                _Client.Connect(_EndPoint);
                _SSLStream = new SslStream(_Client.GetStream(), true, delegate { return true; });
                _SSLStream.AuthenticateAsClient("SharpBooruServer");
                _Reader = new BinaryReader(_SSLStream, Encoding.Unicode);
                _Writer = new BinaryWriter(_SSLStream, Encoding.Unicode);
                _Writer.Write(ClientVersion);
                if (!_Reader.ReadBoolean())
                    throw new BooruProtocol.BooruException("Server version mismatch");
                _Writer.Write(_Username);
                _Writer.Write(_Password);
                if (!_Reader.ReadBoolean())
                    throw new BooruProtocol.BooruException("Login failed");
            }
        }

        private void BeginCommunication(BooruProtocol.Command Command)
        {
            Connect();
            _Writer.Write((byte)Command);
        }

        private void EndCommunication()
        {
            var errorCode = (BooruProtocol.ErrorCode)_Reader.ReadByte();
            if (errorCode != BooruProtocol.ErrorCode.Success)
                throw new BooruProtocol.BooruException(errorCode);
        }

        public BooruPost GetPost(ulong ID)
        {
            BeginCommunication(BooruProtocol.Command.GetPost);
            _Writer.Write(ID);
            EndCommunication();
            BooruPost post = BooruPost.FromReader(_Reader);
            post.Thumbnail = BooruImage.FromReader(_Reader);
            return post;
        }

        public BooruImage GetImage(ulong ID)
        {
            BeginCommunication(BooruProtocol.Command.GetImage);
            _Writer.Write(ID);
            EndCommunication();
            int byteCount = (int)_Reader.ReadUInt32();
            return new BooruImage(_Reader.ReadBytes(byteCount));
        }

        public void GetImage(ref BooruPost Post)
        {
            if (Post.Image == null)
                Post.Image = GetImage(Post.ID);
        }

        public void DeletePost(ulong ID)
        {
            BeginCommunication(BooruProtocol.Command.DeletePost);
            _Writer.Write(ID);
            EndCommunication();
        }

        public void SaveServerBooru() 
        {
            BeginCommunication(BooruProtocol.Command.SaveBooru);
            EndCommunication();
        }

        public List<ulong> Search(string Pattern)
        {
            BeginCommunication(BooruProtocol.Command.Search);
            _Writer.Write(Pattern);
            EndCommunication();
                uint count = _Reader.ReadUInt32();
                List<ulong> IDs = new List<ulong>();
                for (uint i = 0; i < count; i++)
                    IDs.Add(_Reader.ReadUInt64());
                return IDs;
        }

        public void DeleteTag(ulong ID)
        {
            BeginCommunication(BooruProtocol.Command.DeleteTag);
            _Writer.Write(ID);
            EndCommunication();
        }

        public ulong EditTag(ulong ID, BooruTag NewTag)
        {
            BeginCommunication(BooruProtocol.Command.EditTag);
            _Writer.Write(ID);
            NewTag.ToWriter(_Writer);
            EndCommunication();
            return _Reader.ReadUInt64();
        }

        public ulong AddPost(BooruPost NewPost, BooruImage Image)
        {
            BeginCommunication(BooruProtocol.Command.AddPost);
            NewPost.ToWriter(_Writer);
            Image.ToWriter(_Writer);
            EndCommunication();
            return _Reader.ReadUInt64();
        }

        public ulong AddPost(BooruAPIs.BooruAPIPost NewAPIPost)
        {
            NewAPIPost.DownloadImage();
            BeginCommunication(BooruProtocol.Command.AddPost);
            NewAPIPost.ToWriter(_Writer);
            NewAPIPost.Image.ToWriter(_Writer);
            EndCommunication();
            return _Reader.ReadUInt64();
        }

        public void ForceKillServer() { BeginCommunication(BooruProtocol.Command.ForceKillServer); }

        public void Disconnect() { Dispose(); }
        public void Dispose()
        {
            try
            {
                if (_Client.Connected)
                    BeginCommunication(BooruProtocol.Command.Disconnect);
            }
            catch { }
            try
            {
                _Reader.Close();
                _Writer.Close();
                _SSLStream.Close();
                _Client.Close();
            }
            catch { }
            finally { _Client = new TcpClient(); }
        }
    }
}