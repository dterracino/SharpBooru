using System;
using System.IO;
using System.Net;
using System.Text;
using System.Security;
using System.Net.Sockets;
using System.Net.Security;
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
            catch (Exception ex) { _Server.Logger.LogFAILAndException(ex); }
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
            PrepareConnection(_Server.Certificate);
            if (!CheckVersion())
                throw new BooruProtocol.BooruException("Client version mismatch");
            _User = TryLogin();
            while (HandlerStage3()) ;
        }

        private void PrepareConnection(X509Certificate Certificate)
        {
            _SSLStream = new SslStream(_Client.GetStream(), true);
            _SSLStream.AuthenticateAsServer(Certificate, false, System.Security.Authentication.SslProtocols.Tls, false);
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
            switch (command)
            {
                case BooruProtocol.Command.GetPost: //TODO Advanced permission checks
                    {
                        ulong postID = _Reader.ReadUInt64();
                        BooruPost post = _Server.Booru.Posts[postID];
                        if (post != null)
                        {
                            _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                            post.ToWriter(_Writer);
                            _Server.Booru.ReadFile(_Writer, "thumb" + postID);
                        }
                        else _Writer.Write((byte)BooruProtocol.ErrorCode.ResourceNotFound);
                        break;
                    }
                case BooruProtocol.Command.GetImage: //TODO Advanced permission checks
                    {
                        ulong postID = _Reader.ReadUInt64();
                        if (_Server.Booru.Posts.Contains(postID) && File.Exists(_Server.Booru.Folder + "image" + postID))
                        {
                            _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                            _Server.Booru.ReadFile(_Writer, "image" + postID);
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
                    if (string.IsNullOrEmpty(searchPattern))
                    {
                        _Writer.Write((uint)_Server.Booru.Posts.Count);
                        _Server.Booru.Posts.ForEach(x => _Writer.Write(x.ID));
                    }
                    else _Writer.Write((uint)0); //TODO Implement search
                    break;
                case BooruProtocol.Command.Disconnect:
                    return false; //stop handling loop
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
                            long deletedCount = 0;
                            foreach (BooruPost post in _Server.Booru.Posts)
                                deletedCount += post.Tags.Remove(tagID);
                            if (deletedCount < 1)
                                _Writer.Write((byte)BooruProtocol.ErrorCode.ResourceNotFound);
                            else _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                        }
                        else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                        break;
                    }
                case BooruProtocol.Command.EditTag:
                    {
                        ulong tagID = _Reader.ReadUInt64();
                        if (_User.CanEditTags)
                        {
                            BooruTag newTag = BooruTag.FromReader(_Reader);
                            newTag.ID = _Server.Booru.GetNextTagID();
                            long editedCount = 0;
                            foreach (BooruPost post in _Server.Booru.Posts)
                                for (int i = 0; i < post.Tags.Count; i++)
                                    if (post.Tags[i].ID == tagID)
                                    {
                                        post.Tags[i] = newTag;
                                        editedCount++;
                                    }
                            if (editedCount > 0)
                            {
                                _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                                _Writer.Write(newTag.ID);
                            }
                            else _Writer.Write((byte)BooruProtocol.ErrorCode.ResourceNotFound);
                        }
                        else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                        break;
                    }
                case BooruProtocol.Command.AddPost:
                    {
                        BooruPost newPost = BooruPost.FromReader(_Reader);
                        int length = (int)_Reader.ReadUInt32();
                        if (_User.CanAddPosts)
                        {
                            newPost.ID = _Server.Booru.GetNextPostID();
                            //TODO Check Tags and replace with existing
                            if (!_User.AdvancePostControl)
                            {
                                newPost.EditCount = 0;
                                newPost.CreationDate = DateTime.Now;
                                newPost.Owner = _User.Username;
                                newPost.ViewCount = 0;
                            }
                            using (FileStream file = File.Open(_Server.Booru.Folder + "image" + newPost.ID, FileMode.Create, FileAccess.Write, FileShare.Read))
                            {
                                for (int i = 0; i < length / 1024; i++)
                                    file.Write(_Reader.ReadBytes(1024), 0, 1024);
                                file.Write(_Reader.ReadBytes(length % 1024), 0, length % 1024);
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
                        ulong postID = _Reader.ReadUInt64();
                        BooruPost newPost = BooruPost.FromReader(_Reader);
                        if (_User.CanEditPosts)
                        {
                            newPost.ID = _Server.Booru.GetNextPostID();
                            //TODO Check Tags and replace with existing
                            if (_Server.Booru.Posts.Contains(postID))
                            {
                                if (!_User.AdvancePostControl)
                                {
                                    BooruPost oldPost = _Server.Booru.Posts[postID];
                                    newPost.EditCount = oldPost.EditCount + 1;
                                    newPost.CreationDate = oldPost.CreationDate;
                                    newPost.Owner = oldPost.Owner;
                                    newPost.ViewCount = oldPost.ViewCount;
                                }
                                for (int i = 0; i < _Server.Booru.Posts.Count; i++)
                                    if (_Server.Booru.Posts[i].ID == postID)
                                        _Server.Booru.Posts[i] = newPost;
                                _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                                _Writer.Write(newPost.ID);
                            }
                            else _Writer.Write((byte)BooruProtocol.ErrorCode.ResourceNotFound);
                        }
                        else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                        break;
                    }
                case  BooruProtocol.Command.ForceKillServer:
                    if (_User.IsAdmin)
                        System.Diagnostics.Process.GetCurrentProcess().Kill();
                    else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                    break;
                default:
                    _Writer.Write((byte)BooruProtocol.ErrorCode.UnknownError);
                    throw new NotImplementedException();
            }
            return true;
        }
    }
}