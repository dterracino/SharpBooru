using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Net.Security;
using System.Collections.Generic;
using TA.SharpBooru.BooruAPIs;

namespace TA.SharpBooru
{
    public class ClientBooru : IDisposable
    {
        private IPEndPoint _EndPoint;
        private string _Username, _Password;

        private TcpClient _Client;
        private SslStream _SSLStream;
        private BinaryReader _Reader;
        private BinaryWriter _Writer;
        private object _Lock = new object();

        private CountCache<BooruPost> _CachePosts = new CountCache<BooruPost>(400);
        private CountCache<BooruImage> _CacheImgs = new CountCache<BooruImage>(20);

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

        public ClientBooru(IPEndPoint EndPoint, string Username, string Password)
        {
            if (string.IsNullOrWhiteSpace(Username))
                throw new ArgumentException("Username must be non-empty");
            else if (string.IsNullOrEmpty(Password))
                throw new ArgumentException("Password must be non-empty");
            _EndPoint = EndPoint;
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
                Disconnect(false);
                _Client.Connect(_EndPoint);
                _SSLStream = new SslStream(_Client.GetStream(), true, delegate { return true; }); //TODO Client config
                _SSLStream.AuthenticateAsClient("SharpBooruServer");
                _Reader = new BinaryReader(_SSLStream, Encoding.Unicode);
                _Writer = new BinaryWriter(_SSLStream, Encoding.Unicode);
                _Writer.Write(BooruProtocol.ProtocolVersion);
                _Writer.Flush();
                if (!_Reader.ReadBoolean())
                {
                    Disconnect(false);
                    throw new BooruProtocol.BooruException(BooruProtocol.ErrorCode.ProtocolVersionMismatch);
                }
                _Writer.Write(_Username);
                _Writer.Write(_Password);
                _Writer.Flush();
                var loginErrorCode = (BooruProtocol.ErrorCode)_Reader.ReadByte();
                if (loginErrorCode != BooruProtocol.ErrorCode.Success)
                {
                    Disconnect(false);
                    throw new BooruProtocol.BooruException(loginErrorCode);
                }
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
            BooruPost cachedPosts = _CachePosts[ID];
            if (cachedPosts == null)
                lock (_Lock)
                {
                    BeginCommunication(BooruProtocol.Command.GetPost);
                    _Writer.Write(ID);
                    _Writer.Write(IncludeThumbnail);
                    EndCommunication();
                    BooruPost post = BooruPost.FromReader(_Reader);
                    if (IncludeThumbnail)
                    {
                        post.Thumbnail = BooruImage.FromReader(_Reader);
                        _CachePosts.Add(post.ID, post); //only add when thumb is included
                    }
                    return post;
                }
            else return cachedPosts;
        }

        public void GetImage(ref BooruPost Post)
        {
            if (Post.Image == null)
                Post.Image = GetImage(Post.ID);
        }
        public BooruImage GetImage(ulong ID)
        {
            BooruImage cachedImage = _CacheImgs[ID];
            if (cachedImage == null)
                lock (_Lock)
                {
                    BeginCommunication(BooruProtocol.Command.GetImage);
                    _Writer.Write(ID);
                    EndCommunication();
                    cachedImage = BooruImage.FromReader(_Reader);
                    _CacheImgs.Add(ID, cachedImage);
                }
            return cachedImage;
        }

        public BooruImage GetThumbnail(ulong ID)
        {
            BooruPost cachedPost = _CachePosts[ID];
            if (cachedPost == null)
                lock (_Lock)
                {
                    BeginCommunication(BooruProtocol.Command.GetThumbnail);
                    _Writer.Write(ID);
                    EndCommunication();
                    return BooruImage.FromReader(_Reader);
                }
            else return cachedPost.Thumbnail;
        }

        public void DeletePost(BooruPost Post) { DeletePost(Post.ID); }
        public void DeletePost(ulong ID)
        {
            lock (_Lock)
            {
                BeginCommunication(BooruProtocol.Command.DeletePost);
                _Writer.Write(ID);
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

        public void DeleteTag(BooruTag Tag) { DeleteTag(Tag.ID); }
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
                return BooruUser.FromReader(_Reader);
            }
        }

        public void AddPost(ref BooruPost NewPost) { AddPost(ref NewPost, null); }
        public void AddPost(ref BooruPost NewPost, Action<float> ProgressCallback) { NewPost.ID = AddPost(NewPost, ProgressCallback); }
        public ulong AddPost(BooruPost NewPost) { return AddPost(NewPost, null); }
        public ulong AddPost(BooruPost NewPost, Action<float> ProgressCallback)
        {
            lock (_Lock)
            {
                BeginCommunication(BooruProtocol.Command.AddPost);
                NewPost.ToWriter(_Writer);
                NewPost.Image.ToWriter(_Writer, ProgressCallback);
                EndCommunication();
                return _Reader.ReadUInt64();
            }
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
                //uint count = _Reader.ReadUInt32();
                //BooruTagList bTagList = new BooruTagList();
                //for (uint i = 0; i < count; i++)
                //    bTagList.Add(new BooruTag(_Reader.ReadString()));
                //return bTagList;
                return BooruTagList.FromReader(_Reader);
            }
        }

        public void SaveImage(BooruPost Post)
        {
            EditImage(Post.ID, Post.Image);
            Post.Width = (uint)Post.Image.Bitmap.Width;
            Post.Height = (uint)Post.Image.Bitmap.Height;
        }

        //MORE OVERLOADS WITH PROGRESSCALLBACK
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
                Post.ToWriter(_Writer);
                EndCommunication();
            }
        }

        public void AddUser(BooruUser User)
        {
            lock (_Lock)
            {
                BeginCommunication(BooruProtocol.Command.AddUser);
                User.ToWriter(_Writer, true);
                EndCommunication();
            }
        }

        public BooruTag GetTag(string TagString)
        {
            lock (_Lock)
            {
                BeginCommunication(BooruProtocol.Command.GetTag);
                _Writer.Write(true);
                _Writer.Write(TagString);
                EndCommunication();
                return BooruTag.FromReader(_Reader);
            }
        }
        public BooruTag GetTag(ulong ID)
        {
            lock (_Lock)
            {
                BeginCommunication(BooruProtocol.Command.GetTag);
                _Writer.Write(false);
                _Writer.Write(ID);
                EndCommunication();
                return BooruTag.FromReader(_Reader);
            }
        }

        public void DeleteUser(BooruUser User) { DeleteUser(User.Username); }
        public void DeleteUser(string Username)
        {
            lock (_Lock)
            {
                BeginCommunication(BooruProtocol.Command.DeleteUser);
                _Writer.Write(Username);
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

        public Dictionary<string, string> GetBooruMiscOptions()
        {
            lock (_Lock)
            {
                BeginCommunication(BooruProtocol.Command.GetBooruMiscOptions);
                EndCommunication();
                uint count = _Reader.ReadUInt32();
                var optionsDict = new Dictionary<string, string>();
                for (uint i = 0; i < count; i++)
                    optionsDict.Add(_Reader.ReadString(), _Reader.ReadString());
                return optionsDict;
            }
        }

        public List<ulong> FindImageDupes(byte[] ImageHash)
        {
            lock (_Lock)
            {
                BeginCommunication(BooruProtocol.Command.FindImageDupes);
                _Writer.Write((uint)ImageHash.Length);
                _Writer.Write(ImageHash);
                EndCommunication();
                uint count = _Reader.ReadUInt32();
                List<ulong> IDs = new List<ulong>();
                for (uint i = 0; i < count; i++)
                    IDs.Add(_Reader.ReadUInt64());
                return IDs;
            }
        }

        public void ClearCaches()
        {
            _CachePosts.Clear();
            _CacheImgs.Clear();
        }

        public void Dispose() { Disconnect(true); }
        public void Disconnect(bool SendDisconnectCommand)
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
