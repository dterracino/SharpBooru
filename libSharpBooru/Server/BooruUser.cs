using System;
using System.IO;

namespace TA.SharpBooru.Server
{
    public class BooruUser
    {
        public bool IsAdmin;
        public bool CanLoginDirect;
        public bool CanLoginOnline;
        public bool CanAddPosts;
        public bool AdvancePostControl;
        public bool CanDeletePosts;
        public bool CanEditPosts;
        public bool CanEditTags;
        public bool CanDeleteTags;
        //TODO Permissions for Aliases
        //TODO Permissions for Pools

        public ushort MaxRating;

        public string Username;
        public string MD5Password;

        public string Password { set { MD5Password = Helper.MD5(value); } }

        public void ToWriter(BinaryWriter Writer)
        {
            Writer.Write(Username);
            Writer.Write(MD5Password);

            Writer.Write(IsAdmin);
            Writer.Write(CanLoginDirect);
            Writer.Write(CanLoginOnline);
            Writer.Write(CanAddPosts);
            Writer.Write(AdvancePostControl);
            Writer.Write(CanDeletePosts);
            Writer.Write(CanEditPosts);
            Writer.Write(CanEditTags);
            Writer.Write(CanDeleteTags);

            Writer.Write(MaxRating);
        }

        public static BooruUser FromReader(BinaryReader Reader)
        {
            return new BooruUser()
            {
                Username = Reader.ReadString(),
                MD5Password = Reader.ReadString(),

                IsAdmin = Reader.ReadBoolean(),
                CanLoginDirect = Reader.ReadBoolean(),
                CanLoginOnline = Reader.ReadBoolean(),
                CanAddPosts = Reader.ReadBoolean(),
                AdvancePostControl = Reader.ReadBoolean(),
                CanDeletePosts = Reader.ReadBoolean(),
                CanEditPosts = Reader.ReadBoolean(),
                CanEditTags = Reader.ReadBoolean(),
                CanDeleteTags = Reader.ReadBoolean(),

                MaxRating = Reader.ReadUInt16()
            };
        }
    }
}