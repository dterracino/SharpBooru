using System;
using System.Data;
using System.Collections.Generic;

namespace TA.SharpBooru
{
    public class BooruUser : BooruResource, ICloneable
    {
        public ulong ID;
        public bool IsAdmin;
        public bool Enabled;
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

        public string Password { set { MD5Password = Helper.BytesToString(Helper.MD5OfString(value)); } }

        public override void ToWriter(ReaderWriter Writer)
        {
            Writer.Write(MD5Password != null);
            Writer.Write(ID);
            Writer.Write(Username, true);
            if (MD5Password != null)
                Writer.Write(MD5Password, true);

            Writer.Write(IsAdmin);
            Writer.Write(Enabled);
            Writer.Write(CanAddPosts);
            Writer.Write(AdvancePostControl);
            Writer.Write(CanDeletePosts);
            Writer.Write(CanEditPosts);
            Writer.Write(CanEditTags);
            Writer.Write(CanDeleteTags);

            Writer.Write(MaxRating);
        }

        public static BooruUser FromReader(ReaderWriter Reader)
        {
            bool includePassword = Reader.ReadBool();
            return new BooruUser()
            {
                ID = Reader.ReadULong(),
                Username = Reader.ReadString(),
                MD5Password = includePassword ? Reader.ReadString() : null,

                IsAdmin = Reader.ReadBool(),
                Enabled = Reader.ReadBool(),
                CanAddPosts = Reader.ReadBool(),
                AdvancePostControl = Reader.ReadBool(),
                CanDeletePosts = Reader.ReadBool(),
                CanEditPosts = Reader.ReadBool(),
                CanEditTags = Reader.ReadBool(),
                CanDeleteTags = Reader.ReadBool(),

                MaxRating = Reader.ReadUShort()
            };
        }

        public static BooruUser FromRow(DataRow Row)
        {
            if (Row != null)
                return new BooruUser()
                {
                    ID = Convert.ToUInt64(Row["ID"]),
                    Username = Convert.ToString(Row["username"]),
                    MD5Password = Convert.ToString(Row["password"]),
                    IsAdmin = Convert.ToBoolean(Row["isadmin"]),
                    Enabled = Convert.ToBoolean(Row["enabled"]),
                    AdvancePostControl = Convert.ToBoolean(Row["perm_apc"]),
                    CanAddPosts = Convert.ToBoolean(Row["perm_canaddposts"]),
                    CanDeletePosts = Convert.ToBoolean(Row["perm_candeleteposts"]),
                    CanEditPosts = Convert.ToBoolean(Row["perm_caneditposts"]),
                    CanEditTags = Convert.ToBoolean(Row["perm_canedittags"]),
                    CanDeleteTags = Convert.ToBoolean(Row["perm_candeletetags"]),
                    MaxRating = Convert.ToUInt16(Row["max_rating"])
                };
            else return null;
        }

        public Dictionary<string, object> ToDictionary(bool IncludeID)
        {
            var dict = new Dictionary<string, object>()
            {
                { "username", Username },
                { "password", MD5Password },
                { "isadmin", IsAdmin.ToString() },
                { "enabled", Enabled.ToString() },
                { "perm_apc", AdvancePostControl.ToString() },
                { "perm_canaddposts", CanAddPosts.ToString() },
                { "perm_candeleteposts", CanDeletePosts.ToString() },
                { "perm_caneditposts", CanEditPosts.ToString() },
                { "perm_canedittags", CanEditTags.ToString() },
                { "perm_candeletetags", CanDeleteTags.ToString() },
                { "max_rating", MaxRating }
            };
            if (IncludeID)
                dict.Add("id", ID);
            return dict;
        }

        public object Clone() { return this.MemberwiseClone(); }
    }

    public class BooruUserList : List<BooruUser>, ICloneable
    {
        public BooruUser this[string Username]
        {
            get
            {
                foreach (BooruUser user in this)
                    if (user.Username == Username)
                        return user;
                return null;
            }
        }

        public bool Contains(string Username)
        {
            foreach (BooruUser user in this)
                if (user.Username == Username)
                    return true;
            return false;
        }

        public int Remove(string Username) { return this.RemoveAll(x => { return x.Username == Username; }); }

        public void ToWriter(ReaderWriter Writer)
        {
            Writer.Write((uint)this.Count);
            this.ForEach(x => x.ToWriter(Writer));
        }

        public static BooruUserList FromReader(ReaderWriter Reader)
        {
            uint count = Reader.ReadUInt();
            BooruUserList bUserList = new BooruUserList();
            for (uint i = 0; i < count; i++)
                bUserList.Add(BooruUser.FromReader(Reader));
            return bUserList;
        }

        public object Clone()
        {
            BooruUserList cList = new BooruUserList();
            this.ForEach(x => cList.Add(x.Clone() as BooruUser));
            return cList;
        }
    }
}
