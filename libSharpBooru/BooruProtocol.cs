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

            // Post(BooruPost), PostID(ulong)
            GetPost,

            // ImageBytes(byte[]), PostID(ulong)
            GetImage,

            // void, void
            SaveBooru

            /*

            // Private(bool), Rating(byte), Source(string), Comment(string),
            // Width(uint), Height(uint), TagCount(uint), Tags(string)...,
            // ThumbLength(uint), Thumbnail(byte[]), ImageLength(uint), Image(byte[])
            AddPost = 0, 

            // PostID(ulong), Rating(byte), ... no images
            EditPost,

            // PostID(ulong)
            RemovePost,

            // PostID(ulong), ImageLength(uint), Image(byte[])
            EditImage,

            // TagID(ulong), Tag(string), Type(string), Description(string), Color(int)
            EditTag,

            // TagID(ulong)
            RemoveTag,

            //TODO Implement Aliases
            AddAlias, //AliasString - BooruTag
            RemoveAlias //AliasString
            
            */
        }

        public enum ErrorCode : byte
        {
            Success = 0,
            NotFound,
            AccessDenied,
            UnknownError
        }
    }
}
