using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TA.SharpBooru.NetIO.Packets;
using TA.SharpBooru.NetIO.Encryption;
using TA.SharpBooru.NetIO.Packets.CorePackets;
using TA.SharpBooru.NetIO.Packets.BooruPackets;

namespace TA.SharpBooru.Client
{
    public class BooruClient : IDisposable
    {
        public delegate bool CheckFingerprintDelegate(string Fingerprint);

        private AES _AES = null;
        private Logger _Logger;
        private List<ResponseWaiter> _Waiters;
        private TcpClient _Client;
        private Stream _Stream;
        private ReaderWriter _ReaderWriter;
        private Thread _ReceiverThread;
        private uint _CurrentRequestID;

        [MethodImpl(MethodImplOptions.Synchronized)]
        private uint GetNextRequestID()
        {
            if (_CurrentRequestID == uint.MaxValue)
                _CurrentRequestID = 0;
            else _CurrentRequestID++;
            return _CurrentRequestID;
        }

        public BooruClient(Logger Logger = null)
        {
            _Logger = Logger ?? Logger.Null;
            _ReceiverThread = new Thread(ReceiverThreadCode);
        }

        public void Connect(IPEndPoint EndPoint, CheckFingerprintDelegate CheckFingerprintDelegate = null)
        {
            _Logger.LogLine("Connecting to {0} port {1}...", EndPoint.Address, EndPoint.Port);
            _Client = new TcpClient();
            _Client.Connect(EndPoint);
            _Stream = _Client.GetStream();
            _ReaderWriter = new ReaderWriter(_Stream);
            _Waiters = new List<ResponseWaiter>();
            Exception exception = DoHandshake(CheckFingerprintDelegate);
            if (exception != null)
            {
                Disconnect();
                throw exception;
            }
            else _ReceiverThread.Start();
        }

        private Exception DoHandshake(CheckFingerprintDelegate ChkDelegate)
        {
            ushort protocolVersion = Helper.GetVersionMinor();
            _Logger.LogLine("ProtocolVersion is {0}", protocolVersion);
            _ReaderWriter.Write(protocolVersion);
            _ReaderWriter.Flush();
            ushort serverProtocolVersion = _ReaderWriter.ReadUShort();
            if (serverProtocolVersion != protocolVersion)
                return new BooruException(BooruException.ErrorCodes.ProtocolVersionMismatch, string.Format("Server {0} != Client {1}", serverProtocolVersion, protocolVersion));
            Packet4_ServerInfo serverInfo = (Packet4_ServerInfo)Packet.PacketFromReader(_ReaderWriter);
            _Logger.LogPublicFields("ServerInfo", serverInfo);
            using (RSA rsa = new RSA(serverInfo.Modulus, serverInfo.Exponent))
            {
                string fingerprint = rsa.GetFingerprint();
                _Logger.LogLine("Server Fingerprint is {0}", fingerprint);
                bool fingerprintOK = true;
                if (ChkDelegate != null)
                    fingerprintOK = ChkDelegate(rsa.GetFingerprint());
                else _Logger.LogLine("No FingerprintCheckDelegate -> Fingerprint accepted");
                if (fingerprintOK)
                {
                    if (serverInfo.Encryption)
                    {
                        byte[] key = new byte[32];
                        using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
                            rng.GetBytes(key);
                        (new Packet5_Encryption() { Key = rsa.EncryptPublic(key) }).PacketToWriter(_ReaderWriter);
                        _AES = new AES(key);
                    }
                    else (new Packet0_Success()).PacketToWriter(_ReaderWriter);
                }
                else throw new BooruException(BooruException.ErrorCodes.FingerprintNotAccepted);
            }
            _BooruInfo = (BooruInfo)((Packet23_Resource)Packet.PacketFromReader(_ReaderWriter, _AES)).Resource;
            _CurrentUser = (BooruUser)((Packet23_Resource)Packet.PacketFromReader(_ReaderWriter, _AES)).Resource;
            _Logger.LogPublicFields("BooruInfo", _BooruInfo);
            _Logger.LogPublicFields("CurrentUser", _CurrentUser);
            return null;
        }

        public void Dispose() { Disconnect(); }

        public void Disconnect()
        {
            if (_ReceiverThread.IsAlive)
                _ReceiverThread.Abort();
            try
            {
                lock (_ReaderWriter)
                {
                    _ReaderWriter.Write(GetNextRequestID());
                    (new Packet3_Disconnect()).PacketToWriter(_ReaderWriter, _AES);
                }
            }
            catch { }
            lock (_Waiters) //Maybe aborted _ReceiverThread locked _Waiters, Deadlock?
                foreach (ResponseWaiter rWaiter in _Waiters)
                    rWaiter.WaitEvent.Set();
            try { _Client.Close(); }
            catch { }
        }

        private void ReceiverThreadCode()
        {
            try
            {
                while (true)
                {
                    uint requestID = _ReaderWriter.ReadUInt();
                    Packet packet = Packet.PacketFromReader(_ReaderWriter, _AES);
                    ProgressResponse(requestID, packet);
                }
            }
            catch (Exception ex) { _Logger.LogLine("PacketReceiver", ex); }
        }

        private void ProgressResponse(uint RequestID, Packet response)
        {
            lock (_Waiters)
                for (int i = 0; i < _Waiters.Count; i++)
                    if (_Waiters[i].RequestID == RequestID)
                    {
                        _Waiters[i].Response = response;
                        _Waiters[i].WaitEvent.Set();
                        _Waiters.RemoveAt(i);
                        return;
                    }
        }

        private Packet DoRequest(Packet RequestPacket, TimeSpan? Timeout = null)
        {
            uint requestID = GetNextRequestID();
            using (ResponseWaiter waiter = new ResponseWaiter(requestID))
            {
                lock (_Waiters)
                    _Waiters.Add(waiter);
                lock (_ReaderWriter)
                {
                    _ReaderWriter.Write(requestID);
                    RequestPacket.PacketToWriter(_ReaderWriter, _AES);
                }
                if (Timeout.HasValue)
                    waiter.WaitEvent.Wait(Timeout.Value);
                else waiter.WaitEvent.Wait();
                if (waiter.Response.PacketID == 1)
                    throw ((Packet1_Exception)waiter.Response).Exception;
                else return waiter.Response;
            }
        }

        private BooruUser _CurrentUser;
        private BooruInfo _BooruInfo;
        private CountCache<BooruPost> _CachePosts = new CountCache<BooruPost>(400);
        private CountCache<BooruImage> _CacheImgs = new CountCache<BooruImage>(20);
        private List<string> _CacheAllTags = null;

        public BooruUser CurrentUser { get { return _CurrentUser; } }
        public BooruInfo BooruInfo { get { return _BooruInfo; } }

        public void ClearCaches()
        {
            _CachePosts.Clear();
            _CacheImgs.Clear();
            if (_CacheAllTags != null)
            {
                _CacheAllTags.Clear();
                _CacheAllTags = null;
            }
        }

        public BooruPost GetPost(ulong ID, bool IncludeThumbnail = true)
        {
            BooruPost cachedPost = _CachePosts[ID];
            if (cachedPost == null)
            {
                Packet postPacket = DoRequest(new Packet17_GetResource() { ID = ID, Type = Packet17_GetResource.ResourceType.Post });
                BooruPost post = (BooruPost)((Packet23_Resource)postPacket).Resource;
                if (IncludeThumbnail)
                {
                    Packet thumbPacket = DoRequest(new Packet17_GetResource() { ID = ID, Type = Packet17_GetResource.ResourceType.Thumbnail });
                    post.Thumbnail = (BooruImage)((Packet23_Resource)thumbPacket).Resource;
                    _CachePosts.Add(post.ID, post); //only add when thumb is included
                }
                return post;
            }
            else return cachedPost;
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
            {
                Packet imagePacket = DoRequest(new Packet17_GetResource() { ID = ID, Type = Packet17_GetResource.ResourceType.Image });
                cachedImage = (BooruImage)((Packet23_Resource)imagePacket).Resource;
                _CacheImgs.Add(ID, cachedImage);
            }
            return cachedImage;
        }

        public BooruImage GetThumbnail(ulong ID)
        {
            BooruPost cachedPost = _CachePosts[ID];
            if (cachedPost == null)
            {
                Packet thumbPacket = DoRequest(new Packet17_GetResource() { ID = ID, Type = Packet17_GetResource.ResourceType.Thumbnail });
                return (BooruImage)((Packet23_Resource)thumbPacket).Resource;
            }
            else return cachedPost.Thumbnail;
        }

        public void DeletePost(BooruPost Post) { DeletePost(Post.ID); }
        public void DeletePost(ulong ID)
        {
            DoRequest(new Packet19_DeleteResource() { ID = ID, Type = Packet19_DeleteResource.ResourceType.Post });
            _CachePosts.Remove(ID);
            _CacheImgs.Remove(ID);
        }

        public void DeleteTag(BooruTag Tag) { DeleteTag(Tag.ID); }
        public void DeleteTag(ulong ID)
        {
            DoRequest(new Packet19_DeleteResource() { ID = ID, Type = Packet19_DeleteResource.ResourceType.Tag });
            if (_CacheAllTags != null)
                _CacheAllTags.Clear();
        }

        public void AddAlias(string Alias, BooruTag Tag) { AddAlias(Alias, Tag.ID); }
        public void AddAlias(string Alias, string Tag) { AddAlias(Alias, GetTag(Tag)); }
        public void AddAlias(string Alias, ulong TagID) { DoRequest(new Packet27_AddAlias() { Alias = Alias, TagID = TagID }); }

        public BooruTag GetTag(string TagString)
        {
            Packet tagPacket = DoRequest(new Packet17_GetResource() { Type = Packet17_GetResource.ResourceType.Tag, Name = TagString });
            return (BooruTag)((Packet23_Resource)tagPacket).Resource;
        }
        public BooruTag GetTag(ulong ID)
        {
            Packet tagPacket = DoRequest(new Packet17_GetResource() { ID = ID, Type = Packet17_GetResource.ResourceType.Tag });
            return (BooruTag)((Packet23_Resource)tagPacket).Resource;
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
            DoRequest(new Packet20_EditResource() { ID = ID, Type = Packet20_EditResource.ResourceType.Image, Resource = Image });
            _CacheImgs.Remove(ID);
        }

        public void EditPost(ulong ID, BooruPost Post)
        {
            Post.ID = ID;
            SavePost(Post);
        }

        public void SavePost(BooruPost Post)
        {
            DoRequest(new Packet20_EditResource() { ID = Post.ID, Type = Packet20_EditResource.ResourceType.Post, Resource = Post });
            _CachePosts.Remove(Post.ID);
        }

        public void AddUser(BooruUser User) { DoRequest(new Packet21_AddResource() { Type = Packet21_AddResource.ResourceType.User, Resource = User }); }

        public void DeleteUser(BooruUser User) { DeleteUser(User.Username); }
        public void DeleteUser(string Username) { DoRequest(new Packet19_DeleteResource() { Type = Packet19_DeleteResource.ResourceType.User, Name = Username }); }

        public BooruPostList GetPosts(List<ulong> IDs)
        {
            if (IDs == null)
                return new BooruPostList();
            BooruPostList list = new BooruPostList();
            foreach (ulong id in IDs)
                list.Add(GetPost(id));
            return list;
        }

        public List<string> GetAllTags()
        {
            if (_CacheAllTags == null)
            {
                Packet allTagsPacket = DoRequest(new Packet18_GetAllTags());
                _CacheAllTags = ((Packet24_StringList)allTagsPacket).StringList;
            }
            return _CacheAllTags;
        }

        public void IsAlive()
        {
            DoRequest(new Packet2_IsAlive());
        }

        public void AddPost(ref BooruPost NewPost) { AddPost(ref NewPost, null); }
        public void AddPost(ref BooruPost NewPost, Action<float> ProgressCallback) { NewPost.ID = AddPost(NewPost, ProgressCallback); }
        public ulong AddPost(BooruPost NewPost) { return AddPost(NewPost, null); }
        public ulong AddPost(BooruPost NewPost, Action<float> ProgressCallback)
        {
            Packet ulongListPacket = DoRequest(new Packet21_AddResource() { Type = Packet21_AddResource.ResourceType.Post, Resource = NewPost });
            return ((Packet25_ULongList)ulongListPacket).ULongList[0];
        }

        public List<ulong> Search(string Expression)
        {
            Packet resultPacket = DoRequest(new Packet22_Search() { SearchExpression = Expression });
            return ((Packet25_ULongList)resultPacket).ULongList;
        }

        public void EditTag(ulong ID, BooruTag Tag)
        {
            Tag.ID = ID;
            SaveTag(Tag);
        }

        public void SaveTag(BooruTag Tag)
        {
            DoRequest(new Packet20_EditResource() { ID = Tag.ID, Type = Packet20_EditResource.ResourceType.Tag, Resource = Tag });
            if (_CacheAllTags != null)
                _CacheAllTags.Clear();
        }

        public void Login(string Username, string Password)
        {
            Packet currentUserPacket = DoRequest(new Packet16_Login() { Username = Username, Password = Password });
            _CurrentUser = (BooruUser)((Packet23_Resource)currentUserPacket).Resource;
        }

        public List<ulong> FindImageDupes(byte[] ImageHash)
        {
            Packet resultPacket = DoRequest(new Packet26_SearchImg() { ImageHash = ImageHash });
            return ((Packet25_ULongList)resultPacket).ULongList;
        }
    }
}