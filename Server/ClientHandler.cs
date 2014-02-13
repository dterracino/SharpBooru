using System;
using System.Net.Sockets;
using System.Collections.Generic;
using TA.SharpBooru.NetIO.Packets;
using TA.SharpBooru.NetIO.Encryption;
using TA.SharpBooru.NetIO.Packets.CorePackets;
using TA.SharpBooru.NetIO.Packets.BooruPackets;

namespace TA.SharpBooru.Server
{
    public class ClientHandler : IDisposable
    {
        private ServerBooru _Booru;
        private BooruUser _User;

        private bool _ContinueHandling = true;
        private TcpClient _Client;
        private NetworkStream _Stream;
        private ReaderWriter _ReaderWriter;
        private Logger _Logger;
        private AES _AES = null;

        public ClientHandler(Logger Logger, ServerBooru Booru, TcpClient Client)
        {
            _Logger = Logger;
            _Booru = Booru;
            _User = _Booru.Login(null, "guest", "guest");
            _Client = Client;
            _Stream = _Client.GetStream();
            _ReaderWriter = new ReaderWriter(_Stream);
        }

        private void DoHandshake()
        {
            _ReaderWriter.Write(BooruServer.ProtocolVersion);
            _ReaderWriter.Flush();
            ushort clientProtocolVersion = _ReaderWriter.ReadUShort();
            if (clientProtocolVersion != BooruServer.ProtocolVersion)
                throw new BooruException(BooruException.ErrorCodes.ProtocolVersionMismatch, string.Format("Client {0} != Server {1}", clientProtocolVersion, BooruServer.ProtocolVersion));
            byte[] exp, mod;
            _Booru.RSA.GetPublicKey(out mod, out exp);
            Packet4_ServerInfo serverInfo = new Packet4_ServerInfo()
            {
                AdminContact = string.Empty,
                Encryption = true,
                Exponent = exp,
                Modulus = mod,
                ServerName = "SharpBooru Server"
            };
            serverInfo.PacketToWriter(_ReaderWriter);
            Packet response = Packet.PacketFromReader(_ReaderWriter);
            if (response is Packet3_Disconnect)
                throw new BooruException(BooruException.ErrorCodes.FingerprintNotAccepted);
            else if (response is Packet5_Encryption)
            {
                Packet5_Encryption encryptionPacket = (Packet5_Encryption)response;
                byte[] key = _Booru.RSA.DecryptPrivate(encryptionPacket.Key);
                _AES = new AES(key);
            }
            else if (response is Packet0_Success)
            {
                if (serverInfo.Encryption)
                    throw new Exception("Client hasn't exchanged session key");
            }
            else throw new Exception("Packet is no valid response");
            (new Packet23_Resource() { Type = Packet23_Resource.ResourceType.BooruInfo, Resource = _Booru.BooruInfo }).PacketToWriter(_ReaderWriter, _AES, false);
            (new Packet23_Resource() { Type = Packet23_Resource.ResourceType.User, Resource = _User }).PacketToWriter(_ReaderWriter, _AES);
        }

        public void Dispose()
        {
            _ReaderWriter.Dispose();
            _Stream.Dispose();
            _Client.Close();
        }

        public void Handle()
        {
            try { DoHandshake(); }
            catch (Exception ex)
            {
                _Logger.LogException("Handshake", ex);
                _ContinueHandling = false;
            }
            while (_ContinueHandling)
            {
                Packet responsePacket = null;
                try
                {
                    string remote = string.Format("{0} ({1})", _User.Username, _Client.Client.RemoteEndPoint);
                    uint requestID = _ReaderWriter.ReadUInt();
                    using (Packet requestPacket = Packet.PacketFromReader(_ReaderWriter, _AES))
                    {
                        _Logger.LogLine("Got request {0} {1} from {2}", requestID, requestPacket.GetType().Name, remote);
                        try { responsePacket = HandlePacket(requestPacket); }
                        catch (Exception ex)
                        {
                            _Logger.LogException("PacketHandler", ex);
                            responsePacket = new Packet1_Exception() { Exception = ex };
                        }
                    }
                    if (responsePacket != null)
                    {
                        _Logger.LogLine("Sent response {0} {1} to {2}", requestID, responsePacket.GetType().Name, remote);
                        _ReaderWriter.Write(requestID);
                        responsePacket.PacketToWriter(_ReaderWriter, _AES);
                    }
                }
                catch (Exception ex)
                {
                    _Logger.LogException("ClientHandler", ex);
                    _ContinueHandling = false;
                }
                if (responsePacket != null)
                    responsePacket.Dispose();
            }
        }

        private Packet HandlePacket(Packet reqPacket)
        {
            switch (reqPacket.PacketID)
            {
                case 2: break;

                case 3:
                    _Logger.LogLine("{0} ({1}) disconnected", _User.Username, _Client.Client.RemoteEndPoint);
                    _ContinueHandling = false;
                    return null;

                case 16:
                    Packet16_Login packet16 = (Packet16_Login)reqPacket;
                    _User = _Booru.Login(_User, packet16.Username, packet16.Password);
                    return new Packet23_Resource() { Type = Packet23_Resource.ResourceType.User, Resource = _User };

                case 17:
                    Packet17_GetResource packet17 = (Packet17_GetResource)reqPacket;
                    switch (packet17.Type)
                    {
                        case Packet17_GetResource.ResourceType.Post: return new Packet23_Resource() { Type = Packet23_Resource.ResourceType.Post, Resource = _Booru.GetPost(_User, packet17.ID, false) };
                        case Packet17_GetResource.ResourceType.Image: return new Packet23_Resource() { Type = Packet23_Resource.ResourceType.Image, Resource = _Booru.GetImage(_User, packet17.ID) };
                        case Packet17_GetResource.ResourceType.Thumbnail: return new Packet23_Resource() { Type = Packet23_Resource.ResourceType.Thumbnail, Resource = _Booru.GetThumbnail(_User, packet17.ID) };
                        case Packet17_GetResource.ResourceType.Tag:
                            if (packet17.Name != null)
                                return new Packet23_Resource() { Type = Packet23_Resource.ResourceType.Tag, Resource = _Booru.GetTag(_User, packet17.Name) };
                            else return new Packet23_Resource() { Type = Packet23_Resource.ResourceType.Tag, Resource = _Booru.GetTag(_User, packet17.ID) };
                    }
                    break;

                case 18: return new Packet24_StringList() { StringList = _Booru.GetAllTags() };

                case 19:
                    Packet19_DeleteResource packet19 = (Packet19_DeleteResource)reqPacket;
                    switch (packet19.Type)
                    {
                        case Packet19_DeleteResource.ResourceType.Post: _Booru.DeletePost(_User, packet19.ID); break;
                        case Packet19_DeleteResource.ResourceType.Tag: _Booru.DeleteTag(_User, packet19.ID); break;
                        case Packet19_DeleteResource.ResourceType.User: _Booru.DeleteUser(_User, packet19.Name); break;
                    }
                    break;

                case 20:
                    Packet20_EditResource packet20 = (Packet20_EditResource)reqPacket;
                    switch (packet20.Type)
                    {
                        case Packet20_EditResource.ResourceType.Post: _Booru.EditPost(_User, (BooruPost)packet20.Resource); break;
                        case Packet20_EditResource.ResourceType.Tag: _Booru.EditTag(_User, (BooruTag)packet20.Resource); break;
                        case Packet20_EditResource.ResourceType.Image: _Booru.EditImage(_User, packet20.ID, (BooruImage)packet20.Resource); break; //ID field not always used
                    }
                    break;

                case 21:
                    Packet21_AddResource packet21 = (Packet21_AddResource)reqPacket;
                    switch (packet21.Type)
                    {
                        case Packet21_AddResource.ResourceType.Post:
                            ulong addedPostID = _Booru.AddPost(_User, (BooruPost)packet21.Resource);
                            return new Packet25_ULongList() { ULongList = new List<ulong>(1) { addedPostID } };
                        case Packet21_AddResource.ResourceType.User: _Booru.AddUser(_User, (BooruUser)packet21.Resource); break;
                    }
                    break;

                case 22: return new Packet25_ULongList() { ULongList = _Booru.Search(_User, ((Packet22_Search)reqPacket).SearchExpression) };

                case 26: return new Packet25_ULongList() { ULongList = _Booru.SearchImg(_User, ((Packet26_SearchImg)reqPacket).ImageHash) };

                case 27:
                    Packet27_AddAlias packet27 = (Packet27_AddAlias)reqPacket;
                    _Booru.AddAlias(_User, packet27.Alias, packet27.TagID);
                    break;
            }
            return new Packet0_Success();
        }
    }
}