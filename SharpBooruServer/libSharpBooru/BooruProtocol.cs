using System;

namespace TA.SharpBooru
{
    public static class BooruProtocol
    {
        public const ushort ProtocolVersion = 0xB;

        // ReturnParameter(Type), [Argument1(Type)], [Argument2(Type)], ...
        public enum Command : byte
        {
            // IDs(ulong[]), SearchPattern(string)
            Search, //TODO ########## UNTESTED

            // void
            Disconnect,

            // Post(BooruPost) [ + Thumbnail(BooruImage) ], PostID(ulong), IncludeThumbnail(bool)
            GetPost, //TODO ########## UNTESTED

            // Image(BooruImage), PostID(ulong)
            GetImage, //TODO ########## UNTESTED

            // Thunbmail(BooruImage), PostID(ulong)
            GetThumbnail, //TODO ########## UNTESTED

            // void, PostID(ulong)
            DeletePost, //TODO ########## UNTESTED

            // void, TagID(ulong)
            DeleteTag, //TODO ########## UNTESTED

            // void, NewTag(BooruTag) with old ID
            EditTag, //TODO ########## UNTESTED

            // NewPostID(ulong), Post(BooruPost), Image(BooruImage)
            AddPost,

            // void, NewPost(BooruPost) with old ID
            EditPost, //TODO ########## UNTESTED

            // Tags(BooruTagList)
            GetAllTags,

            // User(BooruUser)
            GetCurrentUser,

            // void, Username(string), Password(string)
            ChangeUser,

            // void, PostID(ulong), Image(BooruImage)
            EditImage, //TODO ########## UNTESTED

            // Don't react to this byte
            TestConnection,

            /*
            //TODO Implement Aliases
            AddAlias, //AliasString - BooruTag
            RemoveAlias //AliasString
            */

            // void, NewUser(BooruUser)
            AddUser, //TODO ########## UNTESTED

            // void, Username(string)
            DeleteUser, //TODO ########## UNTESTED

            //GetAllUsers

            // BooruInfo(BooruInfo)
            GetBooruInfo, //TODO ########## UNTESTED

            // DupeIDs(List<ulong>), ulong Hashs
            FindImageDupes //TODO ########## UNTESTED
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
