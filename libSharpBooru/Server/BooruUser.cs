using System;
using System.IO;

namespace TA.SharpBooru.Server
{
    public class BooruUser
    {
        public bool IsAdmin;

        public string Username; 
        public string MD5Password;

        public string Password { set { MD5Password = Helper.MD5(value); } }

        public void ToWriter(BinaryWriter Writer)
        {
            Writer.Write(Username);
            Writer.Write(MD5Password);

            Writer.Write(IsAdmin);
        }

        public static BooruUser FromReader(BinaryReader Reader)
        {
            return new BooruUser()
            {
                Username = Reader.ReadString(),
                MD5Password = Reader.ReadString(),

                IsAdmin = Reader.ReadBoolean()
            };
        }
    }
}
