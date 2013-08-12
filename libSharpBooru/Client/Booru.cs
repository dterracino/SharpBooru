using System;
using System.IO;
using System.Net;
using System.Text;
using System.Security;
using System.Net.Sockets;
using System.Net.Security;
using System.Collections.Generic;
using TA.SharpBooru.BooruAPIs;

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
        private object _Lock = new object();

        private BooruUser _CurrentUser = null;
        public BooruUser CurrentUser
        {
            get
            {
                if (_CurrentUser == null)
                    _CurrentUser = GetCurrentUser();
                return _CurrentUser;
            }
            set { _CurrentUser = value; }
        }

        public Booru(string Server, ushort Port, string Username, string Password)
        {
            if (string.IsNullOrWhiteSpace(Username))
                throw new ArgumentException("Username must be non-empty");
            else if (string.IsNullOrEmpty(Password))
                throw new ArgumentException("Password must be non-empty");
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
            if (_Client.Connected)
                try 
                {
                    _Writer.Write((byte)BooruProtocol.Command.TestConnection); //Test connection (disconnects on fail)
                    _Writer.Flush();
                } 
                catch { }
            if (!_Client.Connected)
            {
                Disconnect();
                _Client.Connect(_EndPoint);
                _SSLStream = new SslStream(_Client.GetStream(), true, delegate { return true; }); //TODO Client config
                _SSLStream.AuthenticateAsClient("SharpBooruServer");
                _Reader = new BinaryReader(_SSLStream, Encoding.Unicode);
                _Writer = new BinaryWriter(_SSLStream, Encoding.Unicode);
                _Writer.Write(ClientVersion);
                _Writer.Flush();
                if (!_Reader.ReadBoolean())
                    throw new BooruProtocol.BooruException("Server version mismatch");
                _Writer.Write(_Username);
                _Writer.Write(_Password);
                _Writer.Flush();
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
            _Writer.Flush();
            var errorCode = (BooruProtocol.ErrorCode)_Reader.ReadByte();
            if (errorCode != BooruProtocol.ErrorCode.Success)
                throw new BooruProtocol.BooruException(errorCode);
        }

        public BooruPost GetPost(ulong ID, bool IncludeThumbnail = true)
        {
            lock (_Lock)
            {
                BeginCommunication(BooruProtocol.Command.GetPost);
                _Writer.Write(ID);
                _Writer.Write(IncludeThumbnail);
                EndCommunication();
                BooruPost post = BooruPost.FromServerReader(_Reader);
                post.Thumbnail = BooruImage.FromReader(_Reader);
                return post;
            }
        }

        public void GetImage(ref BooruPost Post)
        {
            if (Post.Image == null)
                Post.Image = GetImage(Post.ID);
        }
        public BooruImage GetImage(ulong ID)
        {
            lock (_Lock)
            {
                BeginCommunication(BooruProtocol.Command.GetImage);
                _Writer.Write(ID);
                EndCommunication();
                return BooruImage.FromReader(_Reader);
            }
        }

        public BooruImage GetThumbnail(ulong ID)
        {
            lock (_Lock)
            {
                BeginCommunication(BooruProtocol.Command.GetThumbnail);
                _Writer.Write(ID);
                EndCommunication();
                return BooruImage.FromReader(_Reader);
            }
        }

        public void DeletePost(ulong ID)
        {
            lock (_Lock)
            {
                BeginCommunication(BooruProtocol.Command.DeletePost);
                _Writer.Write(ID);
                EndCommunication();
            }
        }

        public void SaveServerBooru()
        {
            lock (_Lock)
            {
                BeginCommunication(BooruProtocol.Command.SaveBooru);
                EndCommunication();
            }
        }

        public List<ulong> Search(string Pattern)
        {
            lock (_Lock)
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
        }

        public void DeleteTag(ulong ID)
        {
            lock (_Lock)
            {
                BeginCommunication(BooruProtocol.Command.DeleteTag);
                _Writer.Write(ID);
                EndCommunication();
            }
        }

        public void EditTag(ulong ID, BooruTag Tag)
        {
            Tag.ID = ID;
            SaveTag(Tag);
        }

        public void SaveTag(BooruTag Tag)
        {
            lock (_Lock)
            {
                BeginCommunication(BooruProtocol.Command.EditTag);
                Tag.ToWriter(_Writer);
                EndCommunication();
            }
        }

        public BooruUser GetCurrentUser()
        {
            lock (_Lock)
            {
                BeginCommunication(BooruProtocol.Command.GetCurrentUser);
                EndCommunication();
                return BooruUser.FromReader(_Reader, false);
            }
        }

        public void AddPost(ref BooruPost NewPost) { NewPost.ID = AddPost(NewPost); }
        public ulong AddPost(BooruPost NewPost) { return AddPost(NewPost, null); }
        public ulong AddPost(BooruPost NewPost, Action<float> ProgressCallback)
        {
            lock (_Lock)
            {
                BeginCommunication(BooruProtocol.Command.AddPost);
                NewPost.ToServerWriter(_Writer);
                NewPost.Image.ToWriter(_Writer, ProgressCallback);
                EndCommunication();
                return _Reader.ReadUInt64();
            }
        }

        public ulong AddPost(BooruAPIPost NewAPIPost) { return AddPost(NewAPIPost, null); }
        public ulong AddPost(BooruAPIPost NewAPIPost, Action<float> ProgressCallback)
        {
            NewAPIPost.DownloadImage();
            return AddPost((BooruPost)NewAPIPost, ProgressCallback);
        }

        public BooruPostList GetPosts(List<ulong> IDs)
        {
            if (IDs == null)
                return new BooruPostList();
            BooruPostList list = new BooruPostList();
            foreach (ulong id in IDs)
                list.Add(GetPost(id));
            return list;
        }

        public BooruTagList GetAllTags()
        {
            lock (_Lock)
            {
                BeginCommunication(BooruProtocol.Command.GetAllTags);
                EndCommunication();
                uint count = _Reader.ReadUInt32();
                BooruTagList bTagList = new BooruTagList();
                for (uint i = 0; i < count; i++)
                    bTagList.Add(new BooruTag(_Reader.ReadString()));
                return bTagList;
            }
        }

        public void ForceKillServer() { BeginCommunication(BooruProtocol.Command.ForceKillServer); }

        public void SaveImage(BooruPost Post) { EditImage(Post.ID, Post.Image); }

        public void EditImage(ulong ID, BooruImage Image)
        {
            lock (_Lock)
            {
                BeginCommunication(BooruProtocol.Command.EditImage);
                _Writer.Write(ID);
                Image.ToWriter(_Writer);
                EndCommunication();
            }
        }

        public void EditPost(ulong ID, BooruPost Post)
        {
            Post.ID = ID;
            SavePost(Post);
        }

        public void SavePost(BooruPost Post)
        {
            lock (_Lock)
            {
                BeginCommunication(BooruProtocol.Command.EditPost);
                Post.ToServerWriter(_Writer);
                EndCommunication();
            }
        }

        public void ChangeUser(string Username, string Password)
        {
            lock (_Lock)
            {
                BeginCommunication(BooruProtocol.Command.ChangeUser);
                _Writer.Write(Username);
                _Writer.Write(Password);
                EndCommunication();
                _CurrentUser = null;
                _Username = Username;
                _Password = Password;
            }
        }

        public void Disconnect() { Dispose(); }
        public void Dispose()
        {
            try
            {
                if (_Client.Connected)
                    lock (_Lock)
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