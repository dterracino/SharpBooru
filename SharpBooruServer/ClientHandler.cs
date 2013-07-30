using System;
using System.IO;
using System.Net;
using System.Text;
using System.Security;
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
            //TODO Moar log output
            try { HandlerStage2(); }
            catch (Exception ex) { _Server.Logger.LogException(ex); }
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
            _User = TryLogin();
            _Server.Logger.LogLine("{0} successfully logged in as {1}", _Address, _User.Username);
            while (HandlerStage3()) ;
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
            return isCorrectVersion;
        }

        private BooruUser TryLogin()
        {
            string username = _Reader.ReadString();
            string password = _Reader.ReadString();
            password = Helper.MD5(password);
            foreach (BooruUser user in _Server.Booru.Users)
                if (user.Username.Equals(username))
                    if (user.MD5Password.Equals(password))
                        if (user.CanLoginDirect)
                        {
                            _Writer.Write(true);
                            return user;
                        }
                        else throw new BooruProtocol.BooruException("User can't login directly");
            _Writer.Write(false);
            throw new BooruProtocol.BooruException("Authentication failed");
        }

        private bool HandlerStage3()
        {
            var command = (BooruProtocol.Command)_Reader.ReadByte();
            _Server.Logger.LogLine("{0} ({1}) executes command {2}", _User.Username, _Address, command.ToString());
            switch (command)
            {
                case BooruProtocol.Command.GetPost:
                    {
                        ulong postID = _Reader.ReadUInt64();
                        BooruPost post = _Server.Booru.Posts[postID];
                        if (post != null)
                        {
                            if (post.Rating <= _User.MaxRating)
                            {
                                _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                                post.ToClientWriter(_Writer, _Server.Booru.Tags);
                                _Server.Booru.ReadFile(_Writer, "thumb" + postID);
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
                            if (_Server.Booru.Posts[postID].Rating <= _User.MaxRating)
                            {
                                _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                                _Server.Booru.ReadFile(_Writer, "image" + postID);
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
                            if (x.Rating <= _User.MaxRating)
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
                        if (_User.CanEditTags)
                        {
                            BooruTag newTag = BooruTag.FromReader(_Reader);
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
                        BooruPost newPost = BooruPost.FromClientReader(_Reader, ref _Server.Booru.Tags);
                        int length = (int)_Reader.ReadUInt32();
                        if (_User.CanAddPosts)
                        {
                            newPost.ID = _Server.Booru.GetNextPostID();
                            if (!_User.AdvancePostControl)
                            {
                                newPost.EditCount = 0;
                                newPost.CreationDate = DateTime.Now;
                                newPost.Owner = _User.Username;
                                newPost.ViewCount = 0;
                            }
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
                        BooruPost newPost = BooruPost.FromClientReader(_Reader, ref _Server.Booru.Tags);
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
                    _Server.Booru.Tags.ToWriter(_Writer);
                    break;
                case BooruProtocol.Command.GetCurrentUser:
                    _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                    _User.ToWriter(_Writer, false);
                    break;
                default:
                    _Writer.Write((byte)BooruProtocol.ErrorCode.UnknownError);
                    throw new NotImplementedException();
            }
            return true;
        }
    }
}