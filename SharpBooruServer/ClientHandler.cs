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
            try { HandlerStage2(); }
            catch { /* TODO Log error */ }
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
                throw new ProtocolViolationException("Client version mismatch");
            _User = TryLogin();
            if (_User == null)
                throw new SecurityException("Client not authenticated");
            while (true)
                HandlerStage3();
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
                    {
                        _Writer.Write(false);
                        return user;
                    }
            _Writer.Write(false);
            return null;
        }

        private void HandlerStage3()
        {
            var command = (BooruProtocol.Command)_Reader.ReadByte();
            switch (command)
            {
                case BooruProtocol.Command.GetPost:
                    {
                        ulong postID = _Reader.ReadUInt64();
                        BooruPost post = _Server.Booru.Posts[postID];
                        if (post != null)
                        {
                            _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                            post.ToWriter(_Writer);
                        }
                        else _Writer.Write((byte)BooruProtocol.ErrorCode.NotFound);
                        break;
                    }
                case BooruProtocol.Command.GetImage:
                    {
                        ulong postID = _Reader.ReadUInt64();
                        if (_Server.Booru.Posts.Contains(postID))
                        {
                            _Writer.Write((byte)BooruProtocol.ErrorCode.Success);
                            _Server.Booru.ReadFile(_Writer, "image" + postID);
                        }
                        else _Writer.Write((byte)BooruProtocol.ErrorCode.NotFound);
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
                default:
                    _Writer.Write((byte)BooruProtocol.ErrorCode.UnknownError);
                    throw new NotImplementedException();
            }
        }
    }
}