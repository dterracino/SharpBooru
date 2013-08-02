using System;
using System.IO;

namespace TA.SharpBooru
{
    public static class BooruProtocol
    {
        // ReturnParameter(Type), [Argument1(Type)], [Argument2(Type)], ...
        public enum Command : byte
        {
            // IDs(ulong[]), SearchPattern(string)
            Search,

            // void
            SaveBooru,

            // void
            ForceKillServer,

            // void
            Disconnect,

            // Post(BooruPost) + Thumbnail(BooruImage), PostID(ulong)
            GetPost,

            // Image(BooruImage), PostID(ulong)
            GetImage,

            // void, PostID(ulong)
            DeletePost,

            // ----- Maybe remove DeleteTag -----
            // void, TagID(ulong)
            DeleteTag,

            // void, NewTag(BooruTag) with old ID
            EditTag,

            // NewPostID(ulong), Post(BooruPost), Image(BooruImage)
            AddPost,

            // void, NewPost(BooruPost) with old ID
            EditPost,

            // Tags(List<string>
            GetAllTags,

            // User(BooruUser)
            GetCurrentUser,

            // void, Username(string), Password(string)
            ChangeUser,

            // void, PostID(ulong), Image(BooruImage)
            EditImage,

            // void //Always return ErrorCode.Success
            TestConnection

            /*
            
            //TODO Implement Aliases
            AddAlias, //AliasString - BooruTag
            RemoveAlias //AliasString
            
            */
        }

        public enum ErrorCode : byte
        {
            Success = 0,
            ResourceNotFound,
            NoPermission,
            UnknownError
        }

        public class BooruException : Exception
        {
            public BooruException(string Message)
                : base(Message) { }

            public BooruException(ErrorCode ErrorCode)
                : this(string.Format("Server returned ErrorCode {0}: {1}", (byte)ErrorCode, ErrorCode)) { }
        }
    }
}
