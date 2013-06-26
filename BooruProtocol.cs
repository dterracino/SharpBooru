using System;
using System.IO;

namespace TEAM_ALPHA.SharpBooru
{
    public class BooruProtocol
    {
        //Command - TargetObject - Payload
        public enum Command : byte
        {
            AddPost = 0, //null - BooruPost
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
    }
}
