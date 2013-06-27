using System;
using System.IO;
using ProtoBuf;

namespace TA.SharpBooru
{
    public static class BooruProtocol
    {
        public enum Command : byte
        {
            Login = 0,
            AddPost, //null - BooruPost
            EditPost, //PostID - BooruPost w. images set to null
            RemovePost, //PostID - null
            GetPost, //PostID - null
            GetImage, //PostID - null
            EditImage, //PostID - ImageBytes
            EditTag, //TagID - BooruTag
            RemoveTag, //TagID
            AddAlias, //AliasString - BooruTag
            RemoveAlias //AliasString
        }

        public enum ErrorCode : byte
        {
            Success = 0,
            AccessDenied,
            NoPermission,
            UnknownError
        }

        [ProtoContract]
        public class Request<TPayload>
        {
            [ProtoMember(1)]
            public Command Command;
            [ProtoMember(2)]
            public ulong Target;
            [ProtoMember(3)]
            public TPayload Payload;
        }

        [ProtoContract]
        public class Response<TPayload>
        {
            [ProtoMember(1)]
            public ErrorCode ErrorCode;
            [ProtoMember(2)]
            public TPayload Payload;
        }

        public static void SendRequest<T>(Stream Stream, Request<T> Packet) { Serializer.Serialize<Request<T>>(Stream, Packet); }

        public static Request<T> ReceiveRequest<T>(Stream Stream) { return Serializer.Deserialize<Request<T>>(Stream); }

        public static void SendResponse<T>(Stream Stream, Response<T> Packet) { Serializer.Serialize<Response<T>>(Stream, Packet); }

        public static Response<T> ReceiveResponse<T>(Stream Stream) { return Serializer.Deserialize<Response<T>>(Stream); }
    }
}
