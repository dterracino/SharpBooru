using System;
using System.Data;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace TEAM_ALPHA.SQLBooru.Server
{
    public class BooruUser
    {
        public string U_Username = null;
        public string U_Password = null;
        public string U_Group = null;

        public bool Perm_Admin = false;
        public bool Perm_Upload = false;
        public bool Perm_Edit = false;
        public bool Perm_Delete = false;
        public string Perm_SearchPrefix = string.Empty;

        internal BooruUser() { }
        public BooruUser(string Username, string Password, string Group)
        {
            if (U_Username == null)
                throw new ArgumentNullException("Username");
            if (U_Password == null)
                throw new ArgumentNullException("Password");
            if (U_Group == null)
                throw new ArgumentNullException("Group");
            U_Username = Username;
            U_Password = Password;
            U_Group = Group;
        }

        public bool? Perm_Get(string PermName)
        {
            if (!PermName.StartsWith("Perm_"))
                PermName = "Perm_" + PermName;
            foreach (FieldInfo Field in typeof(BooruUser).GetFields())
                if (Field.Name == PermName)
                    return (bool)Field.GetValue(this);
            return null;
        }

        /*
        public bool Perm_Set(string PermName, bool Value)
        {
            if (!PermName.StartsWith("Perm_"))
                PermName = "Perm_" + PermName;
            foreach (FieldInfo Field in typeof(BooruUser).GetFields())
                if (Field.Name == PermName)
                {
                    Field.SetValue(this, Value);
                    return true;
                }
            return false;
        }
        */

        public override int GetHashCode() { return U_Username.GetHashCode(); }
        public override bool Equals(object obj) { return this == (BooruUser)obj; }
        public static bool operator !=(BooruUser User1, BooruUser User2) { return !(User1 == User2); }

        public static bool operator ==(BooruUser User1, BooruUser User2)
        {
            if ((object)User1 != null)
                if ((object)User2 != null)
                    return User1.U_Username == User2.U_Username;
            return (object)User1 == (object)User2;
        }
    }

    public class BooruUserManager
    {
        private const string QUERY_INSERT_USER_USERNAME_PASSWORD = "INSERT INTO server_users (username, password) VALUES (?, ?)";
        private const string QUERY_SELECT_USER_USERNAME_PASSWORD = "SELECT * FROM server_users WHERE username = ? AND password = ?";
        private const string QUERY_SELECT_USER_USERNAME = "SELECT * FROM server_users WHERE username = ?";
        private const string QUERY_DELETE_USER_USERNAME = "DELETE FROM server_users WHERE username = ?";
        private const string QUERY_UPDATE_USER_TVALUE_USERNAME = "UPDATE server_users SET {0} = ? WHERE username = ?";

        private const string QUERY_INSERT_GROUP_GROUPNAME = "INSERT INTO server_groups (groupname) VALUES (?)";
        private const string QUERY_DELETE_GROUP_GROUPNAME = "DELETE FROM server_groups WHERE groupname = ?";
        private const string QUERY_UPDATE_GROUP_TVALUE_GROUPNAME = "UPDATE server_groups SET {0} = ? WHERE groupname = ?";
        private const string QUERY_SELECT_GROUP_GROUPNAME = "SELECT * FROM server_groups WHERE groupname = ?";

        private Booru _Booru;

        public BooruUserManager(Booru Booru) { _Booru = Booru; }

        public bool Exists(string Username)
        {
            using (DataTable dt = _Booru.Wrapper.ExecuteTable(QUERY_SELECT_USER_USERNAME, Username))
                return dt.Rows.Count == 1;
        }

        /*
        public void Save(BooruUser User)
        {
            if (User != BooruUser.Guest)
            {
                if (!Exists(User.U_Username))
                    _Booru.Wrapper.ExecuteInt(QUERY_INSERT_USER_USERNAME_PASSWORD, User.U_Username, MD5(User.U_Password));
                _Booru.Wrapper.ExecuteInt(string.Format(QUERY_UPDATE_USER_TVALUE_USERNAME, "group"), User.U_Group, User.U_Username);
                Array.ForEach(typeof(BooruUser).GetFields(), x =>
                    {
                        if (!x.Name.StartsWith("U"))
                            _Booru.Wrapper.ExecuteInt(string.Format(QUERY_UPDATE_GROUP_TVALUE_GROUPNAME, x.Name), x.GetValue(User).ToString(), User.U_Group);
                    });
            }
        }
        */

        /*
        public void Delete(BooruUser User) { Delete(User.U_Username); }
        public void Delete(string Username)
        {
            if (Username != null)
                if (Username != BooruUser.Guest.U_Username)
                    _Booru.Wrapper.ExecuteInt(QUERY_DELETE_USER_USERNAME, Username);
        }
        */

        public void AddUser(string Username, string Password, string Group = null)
        {
            if (Group == null)
                Group = "default";
            if (string.IsNullOrWhiteSpace(Username))
                throw new ArgumentException("Username");
            if (Password == null)
                throw new ArgumentNullException("Password");
            _Booru.Wrapper.ExecuteInt(QUERY_INSERT_USER_USERNAME_PASSWORD, Username, ServerHelper.MD5(Password));
            _Booru.Wrapper.ExecuteInt(string.Format(QUERY_UPDATE_USER_TVALUE_USERNAME, "group"), Group, Username); //TODO Fix this
        }

        private DataTable GetDataTable(string Username, string Password)
        {
            if (Password != null)
                return _Booru.Wrapper.ExecuteTable(QUERY_SELECT_USER_USERNAME_PASSWORD, Username, ServerHelper.MD5(Password));
            else return _Booru.Wrapper.ExecuteTable(QUERY_SELECT_USER_USERNAME, Username);
        }

        public BooruUser LoadUser(string Username, string Password)
        {
            using (DataTable dt = GetDataTable(Username, Password))
                if (dt.Rows.Count == 1)
                {
                    BooruUser bUser = new BooruUser() { U_Username = Username, U_Group = Convert.ToString(dt.Rows[0]["group"]) };
                    if (!string.IsNullOrWhiteSpace(bUser.U_Group))
                        using (DataTable dt2 = _Booru.Wrapper.ExecuteTable(QUERY_SELECT_GROUP_GROUPNAME, bUser.U_Group))
                            if (dt2.Rows.Count == 1)
                                Array.ForEach(typeof(BooruUser).GetFields(), x =>
                                {
                                    if (x.Name.StartsWith("Perm_"))
                                    {
                                        object convertedValue = Convert.ChangeType(dt2.Rows[0][x.Name.ToLower()], x.FieldType);
                                        x.SetValue(bUser, convertedValue);
                                    }
                                });
                    return bUser;
                }
            return null;
        }

        public BooruUser GuestUser { get { return LoadUser("guest", null); } }
    }
}