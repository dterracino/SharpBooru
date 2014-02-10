using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using TA.SharpBooru.NetIO.Packets;
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
        private NetworkStream _NetStream;

        public ClientHandler(ServerBooru Booru, TcpClient Client)
        {
            _Booru = Booru;
            _User = _Booru.Login(null, "guest", "guest");
            _Client = Client;
            _NetStream = _Client.GetStream();
        }

        public void Dispose()
        {
            _NetStream.Dispose();
            _Client.Close();
        }

        public void Handle()
        {
            while (_ContinueHandling)
            {
                using (ReaderWriter readerWriter = new ReaderWriter(_NetStream))
                {
                    Packet responsePacket = null;
                    try
                    {
                        uint requestID = readerWriter.ReadUInt();
                        using (Packet requestPacket = Packet.PacketFromReader(readerWriter))
                        {
                            try { responsePacket = HandlePacket(requestPacket); }
                            catch (Exception ex) { responsePacket = new Packet1_Exception() { Exception = ex }; }
                        }
                        readerWriter.Write(requestID);
                        responsePacket.PacketToWriter(readerWriter);
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
                        case Packet17_GetResource.ResourceType.Tag: return new Packet23_Resource() { Type = Packet23_Resource.ResourceType.Tag, Resource = _Booru.GetTag(_User, packet17.ID) };
                        case Packet17_GetResource.ResourceType.Image: return new Packet23_Resource() { Type = Packet23_Resource.ResourceType.Image, Resource = _Booru.GetImage(_User, packet17.ID) };
                        case Packet17_GetResource.ResourceType.Thumbnail: return new Packet23_Resource() { Type = Packet23_Resource.ResourceType.Thumbnail, Resource = _Booru.GetThumbnail(_User, packet17.ID) };
                    }
                    break;

                case 18: return new Packet24_StringList() { StringList = _Booru.GetAllTags() };

                case 19:
                    Packet19_DeleteResource packet19 = (Packet19_DeleteResource)reqPacket;
                    switch (packet19.Type)
                    {
                        case Packet19_DeleteResource.ResourceType.Post: _Booru.DeletePost(_User, packet19.ID); break;
                        case Packet19_DeleteResource.ResourceType.Tag: _Booru.DeleteTag(_User, packet19.ID); break;
                        //case Packet19_DeleteResource.ResourceType.User: _Booru.DeleteUser(_User, packet19.ID); break;
                        case Packet19_DeleteResource.ResourceType.User: throw new NotImplementedException();
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
                        case Packet21_AddResource.ResourceType.Alias: throw new NotImplementedException();
                    }
                    break;

                case 22: return new Packet25_ULongList() { ULongList = _Booru.Search(_User, ((Packet22_Search)reqPacket).SearchExpression) };

                case 26: return new Packet25_ULongList() { ULongList = _Booru.SearchImg(_User, ((Packet26_SearchImg)reqPacket).ImageHash) };
            }
            return new Packet0_Success();
        }
    }
}