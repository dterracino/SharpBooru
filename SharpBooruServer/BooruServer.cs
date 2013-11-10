﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Data;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net.Security;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace TA.SharpBooru.Server
{
    public class BooruServer : Server
    {
        private TcpListener _Listener;
        private X509Certificate2 _Certificate;
        private ServerBooru _Booru;

        public override string ServerName { get { return "BooruServer"; } }
        public override string ServerInfo { get { return "Port " + (_Listener.LocalEndpoint as IPEndPoint).Port; } }

        public ServerBooru Booru { get { return _Booru; } }
        public X509Certificate2 Certificate { get { return _Certificate; } }

        public BooruServer(ServerBooru Booru, Logger Logger, X509Certificate2 Certificate, ushort Port = 2400)
        {
            base.Logger = Logger;
            _Booru = Booru;
            _Certificate = Certificate;
            _Listener = new TcpListener(IPAddress.Any, Port);
        }

        public override object ConnectClient() { return _Listener.AcceptTcpClient(); }

        public override bool HandleException(System.Exception Ex)
        {
            Logger.LogException(ServerName, Ex);
            return true;
        }

        public override void StartListener() { _Listener.Start(); }

        public override void StopListener() { _Listener.Stop(); }

        public override void HandleClient(object Client)
        {
            using (TcpClient client = Client as TcpClient)
            using (ClientHandler cHandler = new ClientHandler(this, client))
                cHandler.Handle();
        }

        public class ClientHandler : IDisposable
        {
            private BooruServer _Server;
            private TcpClient _Client;
            private SslStream _SSLStream;
            private BooruUser _User;
            private BinaryReader _Reader;
            private BinaryWriter _Writer;
            private IPAddress _Address;

            public ClientHandler(BooruServer Server, TcpClient Client)
            {
                _Server = Server;
                _Client = Client;
            }

            public void Dispose()
            {
                try
                {
                    _Reader.Close();
                    _Writer.Close();
                    _SSLStream.Close();
                    _Client.Close();
                }
                catch { }
            }

            public void Handle()
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
                while (true)
                {
                    bool continueHandling = HandlerStage3();
                    _Writer.Flush();
                    if (!continueHandling)
                        break;
                }
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
                uint clientVersion = _Reader.ReadUInt16();
                bool isCorrectVersion = clientVersion == BooruProtocol.ProtocolVersion;
                _Writer.Write(isCorrectVersion);
                _Writer.Flush();
                return isCorrectVersion;
            }

            private BooruUser TryLogin()
            {
                string username = _Reader.ReadString();
                string password = _Reader.ReadString();
                password = Helper.ByteToString(Helper.MD5OfString(password));
                DataRow userRow = _Server.Booru.DB.ExecuteRow(SQLStatements.GetUserByUsername, username);
                BooruUser user = BooruUser.FromRow(userRow);
                var errorCode = BooruProtocol.ErrorCode.Success;
                if (user != null)
                {
                    if (user.MD5Password == password)
                    {
                        if (!user.CanLoginDirect)
                            errorCode = BooruProtocol.ErrorCode.NoPermission;
                    }
                    else errorCode = BooruProtocol.ErrorCode.LoginFailed;
                }
                else errorCode = BooruProtocol.ErrorCode.ResourceNotFound;
                _Writer.Write((byte)errorCode);
                if (errorCode == BooruProtocol.ErrorCode.Success)
                    return user;
                else throw new BooruProtocol.BooruException(errorCode);
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
                                bool includeThumbnail = _Reader.ReadBoolean();
                                DataRow postRow = _Server.Booru.DB.ExecuteRow(SQLStatements.GetPostByID, postID);
                                BooruPost post = BooruPost.FromRow(postRow);
                                if (post != null)
                                {
                                    DataTable tagTable = _Server.Booru.DB.ExecuteTable(SQLStatements.GetTagsByPostID, postID);
                                    post.Tags = BooruTagList.FromTable(tagTable);
                                    if (post.Rating <= _User.MaxRating && IsPrivacyAllowed(post, _User))
                                    {
                                        _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                                        post.ToWriter(_Writer);
                                        if (includeThumbnail)
                                            _Server.Booru.ReadThumb(_Writer, postID);
                                        //TODO X ViewCount
                                    }
                                    else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                                }
                                else _Writer.Write((byte)BooruProtocol.ErrorCode.ResourceNotFound);
                            }
                            break;
                        case BooruProtocol.Command.GetThumbnail:
                            {
                                ulong postID = _Reader.ReadUInt64();
                                DataRow postRow = _Server.Booru.DB.ExecuteRow(SQLStatements.GetPostByID, postID);
                                BooruPost post = BooruPost.FromRow(postRow);
                                if (post != null)
                                {
                                    if (post.Rating <= _User.MaxRating && IsPrivacyAllowed(post, _User))
                                    {
                                        _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                                        _Server.Booru.ReadThumb(_Writer, postID);
                                    }
                                    else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                                }
                                else _Writer.Write((byte)BooruProtocol.ErrorCode.ResourceNotFound);
                            }
                            break;
                        case BooruProtocol.Command.GetImage:
                            {
                                ulong postID = _Reader.ReadUInt64();
                                DataRow postRow = _Server.Booru.DB.ExecuteRow(SQLStatements.GetPostByID, postID);
                                BooruPost post = BooruPost.FromRow(postRow);
                                if (post != null)
                                {
                                    if (post.Rating <= _User.MaxRating && IsPrivacyAllowed(post, _User))
                                    {
                                        _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                                        _Server.Booru.ReadImage(_Writer, postID);
                                        post.ViewCount++; //TODO X Save post after ViewCount++
                                    }
                                    else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                                }
                                else _Writer.Write((byte)BooruProtocol.ErrorCode.ResourceNotFound);
                            }
                            break;
                        case BooruProtocol.Command.Search:
                            _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                            string searchPattern = _Reader.ReadString().Trim().ToLower();
                            BooruPostList searchedPosts = BooruSearch.DoSearch(searchPattern, _Server.Booru);
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
                                    //TODO X Test ExecuteInt
                                    if (_Server.Booru.DB.ExecuteInt(SQLStatements.GetPostCountByID, postID) > 0)
                                    {
                                        _Server.Booru.DB.ExecuteInt(SQLStatements.DeletePostByID, postID);
                                        _Server.Booru.DB.ExecuteInt(SQLStatements.DeletePostTagsByPostID, postID);
                                        _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                                    }
                                    else _Writer.Write((byte)BooruProtocol.ErrorCode.ResourceNotFound);
                                }
                                else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                            }
                            break;
                        case BooruProtocol.Command.DeleteTag:
                            {
                                ulong tagID = _Reader.ReadUInt64();
                                if (_User.CanDeleteTags)
                                {
                                    //TODO X Test ExecuteInt
                                    if (_Server.Booru.DB.ExecuteInt(SQLStatements.GetTagCountByID, tagID) > 0)
                                    {
                                        _Server.Booru.DB.ExecuteInt(SQLStatements.DeleteTagByID, tagID);
                                        _Server.Booru.DB.ExecuteInt(SQLStatements.DeletePostTagsByTagID, tagID);
                                        _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                                    }
                                    else _Writer.Write((byte)BooruProtocol.ErrorCode.ResourceNotFound);
                                }
                                else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                            }
                            break;
                        case BooruProtocol.Command.EditTag:
                            {
                                BooruTag newTag = BooruTag.FromReader(_Reader);
                                if (_User.CanEditTags)
                                {
                                    //TODO X ExecuteInt works?
                                    if (_Server.Booru.DB.ExecuteInt(SQLStatements.GetTagCountByID, newTag.ID) > 0)
                                    {
                                        _Server.Booru.DB.ExecuteInt(SQLStatements.DeleteTagByID, newTag.ID);
                                        _Server.Booru.DB.ExecuteInsert("tags", newTag.ToDictionary(true));
                                        _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                                    }
                                    else _Writer.Write((byte)BooruProtocol.ErrorCode.ResourceNotFound);
                                }
                                else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                            }
                            break;
                        case BooruProtocol.Command.AddPost:
                            {
                                BooruPost newPost = BooruPost.FromReader(_Reader);
                                int length = (int)_Reader.ReadUInt32();
                                if (_User.CanAddPosts)
                                {
                                    if (!_User.AdvancePostControl)
                                    {
                                        newPost.EditCount = 0;
                                        newPost.CreationDate = DateTime.Now;
                                        newPost.ViewCount = 0;
                                        newPost.User = _User.Username;
                                    }
                                    else if (string.IsNullOrWhiteSpace(newPost.User))
                                        newPost.User = _User.Username;
                                    //TODO Implement bandwidth limit (check length variable)
                                    using (BooruImage bigImage = BooruImage.FromBytes(_Reader.ReadBytes(length)))
                                    {
                                        newPost.Width = (uint)bigImage.Bitmap.Width;
                                        newPost.Height = (uint)bigImage.Bitmap.Height;
                                        newPost.ImageHash = bigImage.CalculateImageHash();
                                        //Insert it into the DB
                                        newPost.ID = (uint)_Server.Booru.DB.ExecuteInsert("posts", newPost.ToDictionary(false));
                                        //Maybe Width + Height checks?
                                        bigImage.Save(Path.Combine(_Server.Booru.ImageFolder, "image" + newPost.ID));
                                        //TODO X ThumbnailSize
                                        //using (BooruImage thumbImage = bigImage.CreateThumbnail(_Server.Booru.Info.ThumbnailSize, false))
                                        using (BooruImage thumbImage = bigImage.CreateThumbnail(256, false))
                                            //TODO X ThumbnailQuality
                                            //thumbImage.Save(Path.Combine(_Server.Booru.ImageFolder, "thumb" + newPost.ID), _Server.Booru.Info.ThumbnailQuality);
                                            thumbImage.Save(Path.Combine(_Server.Booru.ThumbFolder, "thumb" + newPost.ID), 1f);
                                        foreach (BooruTag tag in newPost.Tags)
                                        {
                                            DataRow existingTagRow = _Server.Booru.DB.ExecuteRow(SQLStatements.GetTagByTagString, tag.Tag);
                                            BooruTag existingTag = BooruTag.FromRow(existingTagRow);
                                            if (existingTag == null)
                                            {
                                                BooruTag newTag = new BooruTag(tag.Tag, "Default", "Default", System.Drawing.Color.Black); //TODO X Default Tag
                                                _Server.Booru.DB.ExecuteInt(SQLStatements.InsertPostTag, newPost.ID, _Server.Booru.DB.ExecuteInsert("tags", newTag.ToDictionary(false)));
                                            }
                                            else _Server.Booru.DB.ExecuteInt(SQLStatements.InsertPostTag, newPost.ID, existingTag.ID);
                                        }
                                    }
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
                            }
                            break;
                        case BooruProtocol.Command.EditPost:
                            {
                                BooruPost newPost = BooruPost.FromReader(_Reader);
                                if (_User.CanEditPosts)
                                {
                                    //TODO X ExecuteInt?
                                    if (_Server.Booru.DB.ExecuteInt(SQLStatements.GetPostCountByID, newPost.ID) > 0)
                                    {
                                        if (!_User.AdvancePostControl)
                                        {
                                            DataRow postRow = _Server.Booru.DB.ExecuteRow(SQLStatements.GetPostByID, newPost.ID);
                                            BooruPost oldPost = BooruPost.FromRow(postRow);
                                            newPost.EditCount = oldPost.EditCount + 1;
                                            newPost.CreationDate = oldPost.CreationDate;
                                            newPost.User = oldPost.User;
                                            newPost.ViewCount = oldPost.ViewCount;
                                            //TODO X TAGS
                                        }
                                        _Server.Booru.DB.ExecuteInt(SQLStatements.DeletePostByID, newPost.ID);
                                        _Server.Booru.DB.ExecuteInsert("posts", newPost.ToDictionary(true));
                                        _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                                    }
                                    else _Writer.Write((byte)BooruProtocol.ErrorCode.ResourceNotFound);
                                }
                                else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                            }
                            break;
                        //TODO Replace with GetAllTagsWithLetter or something
                        case BooruProtocol.Command.GetAllTags:
                            {
                                _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                                DataTable tagTable = _Server.Booru.DB.ExecuteTable(SQLStatements.GetTags);
                                BooruTagList.FromTable(tagTable).ToWriter(_Writer);
                            }
                            break;
                        case BooruProtocol.Command.GetCurrentUser:
                            _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                            _User.ToWriter(_Writer, false);
                            break;
                        case BooruProtocol.Command.ChangeUser:
                            string username = _Reader.ReadString();
                            string password = _Reader.ReadString();
                            password = Helper.ByteToString(Helper.MD5OfString(password));
                            DataRow userRow = _Server.Booru.DB.ExecuteRow(SQLStatements.GetUserByUsername, username);
                            BooruUser user = BooruUser.FromRow(userRow);
                            var errorCode = BooruProtocol.ErrorCode.Success;
                            if (user != null)
                            {
                                if (user.MD5Password == password || _User.IsAdmin)
                                {
                                    if (!user.CanLoginDirect)
                                        errorCode = BooruProtocol.ErrorCode.NoPermission;
                                }
                                else errorCode = BooruProtocol.ErrorCode.LoginFailed;
                            }
                            else errorCode = BooruProtocol.ErrorCode.ResourceNotFound;
                            if (errorCode == BooruProtocol.ErrorCode.Success)
                                _User = user;
                            _Writer.Write((byte)errorCode);
                            break;
                        case BooruProtocol.Command.EditImage:
                            {
                                ulong postID = _Reader.ReadUInt64();
                                int length = (int)_Reader.ReadUInt32();
                                BooruProtocol.ErrorCode? error = null;
                                if (_User.CanEditPosts)
                                {
                                    //TODO X ExecuteInt?
                                    if (_Server.Booru.DB.ExecuteInt(SQLStatements.GetPostCountByID, postID) > 0)
                                    {
                                        //TODO Implement bandwidth limit (check length variable)
                                        DataRow postRow = _Server.Booru.DB.ExecuteRow(SQLStatements.GetPostByID, postID);
                                        BooruPost post = BooruPost.FromRow(postRow);
                                        using (BooruImage bigImage = BooruImage.FromBytes(_Reader.ReadBytes(length)))
                                        {
                                            //Maybe Width + Height checks?
                                            bigImage.Save(Path.Combine(_Server.Booru.ImageFolder, "image" + postID));
                                            //TODO X ThumbnailSize
                                            //using (BooruImage thumbImage = bigImage.CreateThumbnail(_Server.Booru.Info.ThumbnailSize, false))
                                            using (BooruImage thumbImage = bigImage.CreateThumbnail(256, false))
                                                //TODO X ThumbnailQuality
                                                //thumbImage.Save(Path.Combine(_Server.Booru.ImageFolder, "thumb" + postID), _Server.Booru.Info.ThumbnailQuality);
                                                thumbImage.Save(Path.Combine(_Server.Booru.ThumbFolder, "thumb" + postID), 1f);
                                            post.Width = (uint)bigImage.Bitmap.Width;
                                            post.Height = (uint)bigImage.Bitmap.Height;
                                            post.ImageHash = bigImage.CalculateImageHash();
                                        }
                                        _Server.Booru.DB.ExecuteInt(SQLStatements.DeletePostByID, postID);
                                        _Server.Booru.DB.ExecuteInsert("posts", post.ToDictionary(true));
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
                                    _Reader.ReadUInt32(); _Reader.ReadUInt32();
                                    _Writer.Write((byte)error.Value);
                                }
                            }
                            break;
                        case BooruProtocol.Command.AddUser:
                            BooruUser userToAdd = BooruUser.FromReader(_Reader);
                            if (_User.IsAdmin)
                            {
                                _Server.Booru.DB.ExecuteInsert("users", userToAdd.ToDictionary(false));
                                _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                            }
                            else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                            break;
                        case BooruProtocol.Command.DeleteUser:
                            string usernameToRemove = _Reader.ReadString();
                            if (_User.IsAdmin)
                            {
                                _Server.Booru.DB.ExecuteInt(SQLStatements.DeleteUserByUsername, usernameToRemove);
                                _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                            }
                            else _Writer.Write((byte)BooruProtocol.ErrorCode.NoPermission);
                            break;
                        case BooruProtocol.Command.GetBooruInfo:
                            _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                            //TODO X GetBooruInfo
                            //_Server.Booru.Info.ToWriter(_Writer);
                            break;
                        case BooruProtocol.Command.FindImageDupes:
                            ulong hash = _Reader.ReadUInt64();
                            _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                            DataTable dupeTable = _Server.Booru.DB.ExecuteTable(SQLStatements.GetDuplicatePostIDs, hash, 12); //2nd Param = Threshold
                            List<ulong> ids = new List<ulong>();
                            foreach (DataRow dupeRow in dupeTable.Rows)
                                ids.Add(Convert.ToUInt64(dupeRow["id"]));
                            _Writer.Write((uint)ids.Count);
                            foreach (ulong id in ids)
                                _Writer.Write(id);
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
                    return Post.User == User.Username;
                else return true;
            }
        }
    }
}