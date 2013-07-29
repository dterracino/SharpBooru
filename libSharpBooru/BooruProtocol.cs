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

            // void, TagID(ulong)
            DeleteTag,

            //TODO !!!!!
            // NewTagID(ulong), NewTag(BooruTag) with old ID
            EditTag,

            // NewPostID(ulong), Post(BooruPost), Image(BooruImage)
            AddPost,

            //TODO !!!!!
            // NewPostID(ulong), NewPost(BooruPost) with old ID
            EditPost,

            // Tags(BooruTagList)
            GetAllTags,

            // void, PostID(ulong), Image(BooruImage)
            EditImage

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
