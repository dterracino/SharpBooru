using System;
using System.IO;
using System.Net;
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
        private const ushort ProtocolVersion = 50;

        private ServerBooru _Booru;
        private BooruUser _User;

        private bool _ContinueHandling = true;
        private TcpClient _Client;
        private NetworkStream _Stream;
        private ReaderWriter _ReaderWriter;

        public ClientHandler(ServerBooru Booru, TcpClient Client)
        {
            _Booru = Booru;
            _User = _Booru.Login(null, "guest", "guest");
            _Client = Client;
            _Stream = _Client.GetStream();
            _ReaderWriter = new ReaderWriter(_Stream);
        }

        private void DoHandshake()
        {
            _ReaderWriter.Write(ProtocolVersion);
            _ReaderWriter.Flush();
            ushort clientProtocolVersion = _ReaderWriter.ReadUShort();
            if (clientProtocolVersion != ProtocolVersion)
                throw new Exception(string.Format("ClientProtocolVersion {0} != ServerProtocolVersion {1}", clientProtocolVersion, ProtocolVersion));
            
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
                throw new Exception("Client declined fingerprint");
            else if (response is Packet5_Encryption)
            {
                Packet5_Encryption encryptionPacket = (Packet5_Encryption)response;
                byte[] key = _Booru.RSA.DecryptPrivate(encryptionPacket.Key);
                //TODO Switch to AES
            }
            else if (response is Packet0_Success)
            {
                if (serverInfo.Encryption)
                    throw new Exception("Client doesn't exchanged session key");
            }
            else throw new Exception("Packet is no valid response");
            (new Packet23_Resource() { Type = Packet23_Resource.ResourceType.BooruInfo, Resource = _Booru.BooruInfo }).PacketToWriter(_ReaderWriter, false);
            (new Packet23_Resource() { Type = Packet23_Resource.ResourceType.User, Resource = _User }).PacketToWriter(_ReaderWriter);
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
                //TODO Handle exception
                _ContinueHandling = false;
            }
            while (_ContinueHandling)
            {
                Packet responsePacket = null;
                try
                {
                    uint requestID = _ReaderWriter.ReadUInt();
                    using (Packet requestPacket = Packet.PacketFromReader(_ReaderWriter))
                    {
                        try { responsePacket = HandlePacket(requestPacket); }
                        catch (Exception ex) { responsePacket = new Packet1_Exception() { Exception = ex }; }
                    }
                    _ReaderWriter.Write(requestID);
                    responsePacket.PacketToWriter(_ReaderWriter);
                }
                catch (Exception ex)
                {
                    //TODO Handle exception
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

                case 3: _ContinueHandling = false; break;

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