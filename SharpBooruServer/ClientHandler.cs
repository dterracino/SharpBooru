using System;
using System.IO;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Amib.Threading;

namespace TA.SharpBooru.Server
{
    public class ClientHandler
    {
        private BooruServer _Server;
        private TcpClient _Client;
        private SslStream _SSLStream;
        private BooruUser _User;
        private BinaryReader _Reader;
        private BinaryWriter _Writer;
        private IPAddress _Address;

        private object _GenerateThumbnailLock = new object();

        public ClientHandler(BooruServer Server, TcpClient Client)
        {
            _Server = Server;
            _Client = Client;
        }

        public void Queue(SmartThreadPool ThreadPool) { ThreadPool.QueueWorkItem(HandlerStage1); }

        private object HandlerStage1(object obj)
        {
            try { HandlerStage2(); }
            catch (Exception ex) { _Server.Logger.LogException("HandlerStage1", ex); }
            finally
            {
                _Reader.Close();
                _Writer.Close();
                _SSLStream.Close();
                _Client.Close();
            }
            return true;
        }

        private void HandlerStage2()
        {
            _Address = (_Client.Client.RemoteEndPoint as IPEndPoint).Address;
            _Server.Logger.LogLine("{0} is connecting...", _Address);
            PrepareConnection(_Server.Certificate);
            if (!CheckVersion())
                throw new BooruProtocol.BooruException("Client version mismatch");
            _Server.Logger.LogLine("{0} connected", _Address);
            try { _User = TryLogin(); }
            finally { _Writer.Flush(); }
            _Server.Logger.LogLine("{0} successfully logged in as {1}", _Address, _User.Username);
            while (HandlerStage3())
                _Writer.Flush();
            _Server.Logger.LogLine("{0} ({1}) disconnected", _User.Username, _Address);
        }

        private void PrepareConnection(X509Certificate Certificate)
        {
            _SSLStream = new SslStream(_Client.GetStream(), true);
            _SSLStream.AuthenticateAsServer(Certificate, false, SslProtocols.Tls, false);
            _Reader = new BinaryReader(_SSLStream, Encoding.Unicode);
            _Writer = new BinaryWriter(_SSLStream, Encoding.Unicode);
        }

        private bool CheckVersion()
        {
            uint clientVersion = _Reader.ReadUInt32();
            bool isCorrectVersion = clientVersion == BooruServer.ServerVersion;
            _Writer.Write(isCorrectVersion);
            _Writer.Flush();
            return isCorrectVersion;
        }

        private BooruUser TryLogin()
        {
            string username = _Reader.ReadString();
            string password = _Reader.ReadString();
            password = Helper.ByteToString(Helper.MD5OfString(password));
            foreach (BooruUser user in _Server.Booru.Users)
                if (user.Username == username)
                    if (user.MD5Password == password)
                        if (user.CanLoginDirect)
                        {
                            _Writer.Write(true);
                            return user;
                        }
                        else
                        {
                            _Writer.Write(false);
                            throw new BooruProtocol.BooruException("User can't login directly");
                        }
            _Writer.Write(false);
            throw new BooruProtocol.BooruException("Authentication failed");
        }

        private bool HandlerStage3()
        {
            var command = (BooruProtocol.Command)_Reader.ReadByte();
            if (command != BooruProtocol.Command.TestConnection)
            {
                _Server.Logger.LogLine("{0} ({1}) executes command {2}", _User.Username, _Address, command.ToString());
                switch (command)
                {
                    case BooruProtocol.Command.GetPost:
                        {
                            ulong postID = _Reader.ReadUInt64();
                            BooruPost post = _Server.Booru.Posts[postID];
                            if (post != null)
                            {
                                if (post.Rating <= _User.MaxRating && IsPrivacyAllowed(post, _User))
                                {
                                    _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                                    post.ToClientWriter(_Writer, _Server.Booru);
                                    if (_Reader.ReadBoolean())
                                        _Server.Booru.ReadFile(_Writer, "thumb" + postID);
                                }
                                else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                            }
                            else _Writer.Write((byte)BooruProtocol.ErrorCode.ResourceNotFound);
                            break;
                        }
                    case BooruProtocol.Command.GetThumbnail:
                        {
                            ulong postID = _Reader.ReadUInt64();
                            if (_Server.Booru.Posts.Contains(postID) && File.Exists(Path.Combine(_Server.Booru.Folder, "thumb" + postID)))
                            {
                                BooruPost post = _Server.Booru.Posts[postID];
                                if (post.Rating <= _User.MaxRating && IsPrivacyAllowed(post, _User))
                                {
                                    _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                                    _Server.Booru.ReadFile(_Writer, "thumb" + postID);
                                    post.ViewCount++;
                                }
                                else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                            }
                            else _Writer.Write((byte)BooruProtocol.ErrorCode.ResourceNotFound);
                            break;
                        }
                    case BooruProtocol.Command.GetImage:
                        {
                            ulong postID = _Reader.ReadUInt64();
                            if (_Server.Booru.Posts.Contains(postID) && File.Exists(Path.Combine(_Server.Booru.Folder, "image" + postID)))
                            {
                                BooruPost post = _Server.Booru.Posts[postID];
                                if (post.Rating <= _User.MaxRating && IsPrivacyAllowed(post, _User))
                                {
                                    _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                                    _Server.Booru.ReadFile(_Writer, "image" + postID);
                                    post.ViewCount++;
                                }
                                else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                            }
                            else _Writer.Write((byte)BooruProtocol.ErrorCode.ResourceNotFound);
                            break;
                        }
                    case BooruProtocol.Command.SaveBooru:
                        _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                        _Server.Booru.SaveToDisk();
                        break;
                    case BooruProtocol.Command.Search:
                        _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                        string searchPattern = _Reader.ReadString().Trim().ToLower();
                        BooruPostList searchedPosts = BooruSearch.DoSearch(searchPattern, _Server.Booru.Posts, _Server.Booru.Tags);
                        BooruPostList postsToSend = new BooruPostList();
                        searchedPosts.ForEach(x =>
                            {
                                if (x.Rating <= _User.MaxRating && IsPrivacyAllowed(x, _User))
                                    postsToSend.Add(x);
                            });
                        postsToSend.Sort((b1, b2) => DateTime.Compare(b2.CreationDate, b1.CreationDate));
                        _Writer.Write((uint)postsToSend.Count);
                        postsToSend.ForEach(x => _Writer.Write(x.ID));
                        break;
                    case BooruProtocol.Command.Disconnect: return false; //stop handling loop
                    case BooruProtocol.Command.DeletePost:
                        {
                            ulong postID = _Reader.ReadUInt64();
                            if (_User.CanDeletePosts)
                            {
                                if (_Server.Booru.Posts.Contains(postID))
                                {
                                    _Server.Booru.Posts.Remove(postID);
                                    _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                                }
                                else _Writer.Write((byte)BooruProtocol.ErrorCode.ResourceNotFound);
                            }
                            else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                            break;
                        }
                    case BooruProtocol.Command.DeleteTag:
                        {
                            ulong tagID = _Reader.ReadUInt64();
                            if (_User.CanDeleteTags)
                            {
                                if (_Server.Booru.Tags.Remove(tagID) > 0)
                                {
                                    foreach (BooruPost post in _Server.Booru.Posts)
                                        post.TagIDs.Remove(tagID);
                                    _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                                }
                                else _Writer.Write((byte)BooruProtocol.ErrorCode.ResourceNotFound);
                            }
                            else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                            break;
                        }
                    case BooruProtocol.Command.EditTag:
                        {
                            BooruTag newTag = BooruTag.FromReader(_Reader);
                            if (_User.CanEditTags)
                            {
                                if (_Server.Booru.Tags.Contains(newTag.ID))
                                {
                                    _Server.Booru.Tags.Remove(newTag.ID);
                                    _Server.Booru.Tags.Add(newTag);
                                    _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                                }
                                else _Writer.Write((byte)BooruProtocol.ErrorCode.ResourceNotFound);
                            }
                            else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                            break;
                        }
                    case BooruProtocol.Command.AddPost:
                        {
                            BooruPost newPost = BooruPost.FromClientReader(_Reader, _Server.Booru);
                            int length = (int)_Reader.ReadUInt32();
                            if (_User.CanAddPosts)
                            {
                                newPost.ID = _Server.Booru.GetNextPostID();
                                if (!_User.AdvancePostControl)
                                {
                                    newPost.EditCount = 0;
                                    newPost.CreationDate = DateTime.Now;
                                    newPost.ViewCount = 0;
                                    newPost.Owner = _User.Username;
                                }
                                else if (string.IsNullOrWhiteSpace(newPost.Owner))
                                    newPost.Owner = _User.Username;
                                //TODO Implement bandwidth limit (check length variable)
                                using (BooruImage bigImage = new BooruImage(_Reader.ReadBytes(length)))
                                {
                                    bigImage.Save(Path.Combine(_Server.Booru.Folder, "image" + newPost.ID));
                                    using (BooruImage thumbImage = bigImage.CreateThumbnail(256)) //TODO ThumbSize 
                                        thumbImage.Save(Path.Combine(_Server.Booru.Folder, "thumb" + newPost.ID));
                                }
                                _Server.Booru.Posts.Add(newPost);
                                _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                                _Writer.Write(newPost.ID);
                            }
                            else
                            {
                                for (int i = 0; i < length / 1024; i++)
                                    _Reader.ReadBytes(1024);
                                _Reader.ReadBytes(length % 1024);
                                _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                            }
                            break;
                        }
                    case BooruProtocol.Command.EditPost:
                        {
                            BooruPost newPost = BooruPost.FromClientReader(_Reader, _Server.Booru);
                            if (_User.CanEditPosts)
                            {
                                if (_Server.Booru.Posts.Contains(newPost.ID))
                                {
                                    if (!_User.AdvancePostControl)
                                    {
                                        BooruPost oldPost = _Server.Booru.Posts[newPost.ID];
                                        newPost.EditCount = oldPost.EditCount + 1;
                                        newPost.CreationDate = oldPost.CreationDate;
                                        newPost.Owner = oldPost.Owner;
                                        newPost.ViewCount = oldPost.ViewCount;
                                    }
                                    _Server.Booru.Posts.Remove(newPost.ID);
                                    _Server.Booru.Posts.Add(newPost);
                                    _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                                }
                                else _Writer.Write((byte)BooruProtocol.ErrorCode.ResourceNotFound);
                            }
                            else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                            break;
                        }
                    case BooruProtocol.Command.ForceKillServer:
                        if (_User.IsAdmin)
                            Process.GetCurrentProcess().Kill(); //YOLO
                        else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                        break;
                    case BooruProtocol.Command.GetAllTags:
                        _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                        _Writer.Write((uint)_Server.Booru.Tags.Count);
                        foreach (BooruTag bTag in _Server.Booru.Tags)
                            _Writer.Write(bTag.Tag);
                        break;
                    case BooruProtocol.Command.GetCurrentUser:
                        _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                        _User.ToWriter(_Writer, false);
                        break;
                    case BooruProtocol.Command.ChangeUser:
                        string username = _Reader.ReadString();
                        string password = _Reader.ReadString();
                        password = Helper.ByteToString(Helper.MD5OfString(password));
                        bool newUserLoggedIn = false;
                        foreach (BooruUser user in _Server.Booru.Users)
                            if (user.Username == username)
                                if (user.MD5Password == password)
                                    if (user.CanLoginDirect)
                                    {
                                        _User = user;
                                        newUserLoggedIn = true;
                                    }
                        if (newUserLoggedIn)
                            _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                        else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                        break;
                    case BooruProtocol.Command.EditImage:
                        {
                            ulong postID = _Reader.ReadUInt64();
                            int length = (int)_Reader.ReadUInt32();
                            BooruProtocol.ErrorCode? error = null;
                            if (_User.CanEditPosts)
                            {
                                if (_Server.Booru.Posts.Contains(postID))
                                {
                                    //TODO Implement bandwidth limit (check length variable)
                                    using (BooruImage bigImage = new BooruImage(_Reader.ReadBytes(length)))
                                    {
                                        bigImage.Save(Path.Combine(_Server.Booru.Folder, "image" + postID));
                                        using (BooruImage thumbImage = bigImage.CreateThumbnail(256)) //TODO ThumbSize 
                                            thumbImage.Save(Path.Combine(_Server.Booru.Folder, "thumb" + postID));
                                    }
                                    _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                                }
                                else error = BooruProtocol.ErrorCode.ResourceNotFound;
                            }
                            else error = BooruProtocol.ErrorCode.NoPermission;
                            if (error.HasValue)
                            {
                                for (int i = 0; i < length / 1024; i++)
                                    _Reader.ReadBytes(1024);
                                _Reader.ReadBytes(length % 1024);
                                _Writer.Write((byte)error.Value);
                            }
                        }
                        break;
                    default:
                        _Writer.Write((byte)BooruProtocol.ErrorCode.UnknownError);
                        throw new NotImplementedException();
                }
            }
            return true;
        }

        private bool IsPrivacyAllowed(BooruPost Post, BooruUser User)
        {
            if (Post.Private && !User.IsAdmin)
                return Post.Owner == User.Username;
            else return true;
        }
    }
}