using System;

namespace TA.SharpBooru
{
    public static class BooruProtocol
    {
        public const ushort ProtocolVersion = 9;

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

            // Post(BooruPost) [ + Thumbnail(BooruImage) ], PostID(ulong), IncludeThumbnail(bool)
            GetPost,

            // Image(BooruImage), PostID(ulong)
            GetImage,

            // Thunbmail(BooruImage), PostID(ulong)
            GetThumbnail,

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

            // Don't react to this byte
            TestConnection,

            /*
            
            //TODO Implement Aliases
            AddAlias, //AliasString - BooruTag
            RemoveAlias //AliasString
            
            */

            // void, NewUser(BooruUser)
            AddUser,

            // void, Username(string)
            DeleteUser,

            //GetAllUsers

            // BooruInfo(BooruInfo)
            GetBooruInfo,

            // DupeIDs(List<ulong>), ulong Hashs
            FindImageDupes
        }

        public enum ErrorCode : byte
        {
            Success = 0,
            ResourceNotFound,
            NoPermission,
            UnknownError,
            LoginFailed,
            ProtocolVersionMismatch
        }

        public class BooruException : Exception
        {
            private ErrorCode? _ErrorCode;

            public ErrorCode ErrorCode
            {
                get
                {
                    if (_ErrorCode.HasValue)
                        return _ErrorCode.Value;
                    else return BooruProtocol.ErrorCode.UnknownError;
                }
            }

            public BooruException(string Message)
                : base(Message) { }

            public BooruException(ErrorCode ErrorCode)
                : base(string.Format("ErrorCode {0}: {1}", (byte)ErrorCode, ErrorCode))
            { _ErrorCode = ErrorCode; }
        }
    }
}
