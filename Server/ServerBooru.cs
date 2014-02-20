using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using TA.SharpBooru.NetIO.Encryption;

namespace TA.SharpBooru.Server
{
    public class ServerBooru : IDisposable
    {
        private SQLiteWrapper _DB;
        private BooruInfo _BooruInfo = null;
        private string _Folder;
        private RSA _RSA;

        public BooruInfo BooruInfo
        {
            get
            {
                if (_BooruInfo == null)
                    using (DataTable optionsTable = _DB.ExecuteTable(SQLStatements.GetOptions))
                        _BooruInfo = BooruInfo.FromTable(optionsTable);
                return _BooruInfo;
            }
        }
        //public string Folder { get { return _Folder; } }
        public string ImageFolder { get { return Path.Combine(_Folder, "images"); } }
        public string ThumbFolder { get { return Path.Combine(_Folder, "thumbs"); } }
        //public string AvatarFolder { get { return Path.Combine(_Folder, "avatars"); } }
        public SQLiteWrapper DB { get { return _DB; } }
        public RSA RSA { get { return _RSA; } }

        public ServerBooru(string Folder)
        {
            string dbPath = Path.Combine(Folder, "booru.db");
            if (File.Exists(dbPath))
                if (Directory.Exists(Path.Combine(Folder, "images")))
                    if (Directory.Exists(Path.Combine(Folder, "thumbs")))
                    //if (Directory.Exists(Path.Combine(Folder, "avatars")))
                    {
                        _Folder = Folder;
                        _DB = new SQLiteWrapper(dbPath);
                        string rsaPath = Path.Combine(Folder, "rsa.xml");
                        _RSA = new RSA();
                        if (File.Exists(rsaPath))
                            _RSA.LoadKeys(rsaPath);
                        else _RSA.SaveKeys(rsaPath);
                        return;
                    }
            throw new Exception("No valid booru directory");
        }

        public void Dispose()
        {
            _DB.Dispose();
            _RSA.Dispose();
        }

        public BooruPost GetPost(BooruUser User, ulong PostID, bool IncludeThumbnail)
        {
            DataRow postRow = _DB.ExecuteRow(SQLStatements.GetPostByID, PostID);
            BooruPost post = BooruPost.FromRow(postRow);
            if (post != null)
            {
                DataTable tagTable = _DB.ExecuteTable(SQLStatements.GetTagsByPostID, PostID);
                post.Tags = BooruTagList.FromTable(tagTable);
                if (post.Rating <= User.MaxRating && IsPrivacyAllowed(post, User))
                    return post;
                else throw new Exception("No permission");
            }
            else throw new Exception("Not found");
        }

        public BooruImage GetThumbnail(BooruUser User, ulong PostID)
        {
            DataRow postRow = _DB.ExecuteRow(SQLStatements.GetPostByID, PostID);
            BooruPost post = BooruPost.FromRow(postRow);
            if (post != null)
            {
                if (post.Rating <= User.MaxRating && IsPrivacyAllowed(post, User))
                    return BooruImage.FromFile(Path.Combine(ThumbFolder, "thumb" + PostID));
                else throw new Exception("No permission");
            }
            else throw new Exception("Not found");
        }

        public BooruImage GetImage(BooruUser User, ulong PostID)
        {
            DataRow postRow = _DB.ExecuteRow(SQLStatements.GetPostByID, PostID);
            BooruPost post = BooruPost.FromRow(postRow);
            if (post != null)
            {
                if (post.Rating <= User.MaxRating && IsPrivacyAllowed(post, User))
                {
                    BooruImage image = BooruImage.FromFile(Path.Combine(ImageFolder, "image" + PostID));
                    _DB.ExecuteNonQuery(SQLStatements.UpdateIncrementViewCount, PostID);
                    return image;
                }
                else throw new Exception("No permission");
            }
            else throw new Exception("Not found");
        }

        public List<ulong> Search(BooruUser User, string SearchExpression)
        {
            BooruPostList searchedPosts = BooruSearch.DoSearch(SearchExpression, this);
            BooruPostList postsToSend = new BooruPostList();
            searchedPosts.ForEach(x =>
            {
                if (x.Rating <= User.MaxRating && IsPrivacyAllowed(x, User))
                    postsToSend.Add(x);
            });
            postsToSend.Sort((b1, b2) => DateTime.Compare(b2.CreationDate, b1.CreationDate));
            List<ulong> ids = new List<ulong>(postsToSend.Count);
            postsToSend.ForEach(x => ids.Add(x.ID));
            return ids;
        }

        public void DeletePost(BooruUser User, ulong PostID)
        {
            if (User.CanDeletePosts)
            {
                if (_DB.ExecuteScalar<int>(SQLStatements.GetPostCountByID, PostID) > 0)
                {
                    _DB.ExecuteNonQuery(SQLStatements.DeletePostByID, PostID);
                    _DB.ExecuteNonQuery(SQLStatements.DeletePostTagsByPostID, PostID);
                    File.Delete(Path.Combine(ThumbFolder, "thumb" + PostID));
                    File.Delete(Path.Combine(ImageFolder, "image" + PostID));
                }
                else throw new Exception("Not found");
            }
            else throw new Exception("No permission");
        }

        public void DeleteTag(BooruUser User, ulong TagID)
        {
            if (User.CanDeleteTags)
            {
                if (_DB.ExecuteScalar<int>(SQLStatements.GetTagCountByID, TagID) > 0)
                {
                    _DB.ExecuteNonQuery(SQLStatements.DeleteTagByID, TagID);
                    _DB.ExecuteNonQuery(SQLStatements.DeletePostTagsByTagID, TagID);
                }
                else throw new Exception("Not found");
            }
            else throw new Exception("No permission");
        }

        public BooruTag GetTag(BooruUser User, string Tag)
        {
            DataRow tagRow = _DB.ExecuteRow(SQLStatements.GetTagByTagString, Tag);
            BooruTag tag = BooruTag.FromRow(tagRow);
            if (tag != null)
                return tag;
            else throw new Exception("Not found");
        }

        public BooruTag GetTag(BooruUser User, ulong TagID)
        {
            DataRow tagRow = _DB.ExecuteRow(SQLStatements.GetTagByID, TagID);
            BooruTag tag = BooruTag.FromRow(tagRow);
            if (tag != null)
                return tag;
            else throw new Exception("Not found");
        }

        public void AddAlias(BooruUser User, string Alias, ulong TagID)
        {
            if (User.CanEditTags) //TODO Alias permission?
            {
                if (_DB.ExecuteScalar<int>(SQLStatements.GetTagCountByID, TagID) > 0)
                    _DB.ExecuteNonQuery(SQLStatements.InsertAlias, Alias, TagID);
                else throw new Exception("Not found");
            }
            else throw new Exception("No permission");
        }

        public void AddUser(BooruUser User, BooruUser NewUser)
        {
            if (User.IsAdmin)
                _DB.ExecuteInsert("users", NewUser.ToDictionary(false));
            else throw new Exception("No permission");
        }

        public void DeleteUser(BooruUser User, string Username)
        {
            if (User.IsAdmin)
                _DB.ExecuteNonQuery(SQLStatements.DeleteUserByUsername, Username);
            else throw new Exception("No permission");
        }

        public void EditImage(BooruUser User, ulong PostID, BooruImage Image)
        {
            if (User.CanEditPosts)
            {
                DataRow postRow = _DB.ExecuteRow(SQLStatements.GetPostByID, PostID);
                BooruPost post = BooruPost.FromRow(postRow);
                if (post != null)
                {
                    //Maybe Width + Height checks?
                    Image.Save(Path.Combine(ImageFolder, "image" + PostID));
                    using (BooruImage thumbImage = Image.CreateThumbnail(BooruInfo.ThumbnailSize, false))
                        thumbImage.Save(Path.Combine(ThumbFolder, "thumb" + PostID), BooruInfo.ThumbnailQuality);
                    post.Width = (uint)Image.Bitmap.Width;
                    post.Height = (uint)Image.Bitmap.Height;
                    post.ImageHash = Image.CalculateImageHash();
                    _DB.ExecuteNonQuery(SQLStatements.DeletePostByID, PostID);
                    _DB.ExecuteInsert("posts", post.ToDictionary(true));
                }
                else throw new Exception("Not found");
            }
            else throw new Exception("No permission");
        }

        public ulong AddPost(BooruUser User, BooruPost PostWithImage)
        {
            if (User.CanAddPosts)
            {
                if (!User.AdvancePostControl)
                {
                    PostWithImage.EditCount = 0;
                    PostWithImage.CreationDate = DateTime.Now;
                    PostWithImage.ViewCount = 0;
                    PostWithImage.User = User.Username;
                }
                else if (string.IsNullOrWhiteSpace(PostWithImage.User))
                    PostWithImage.User = User.Username;
                PostWithImage.Width = (uint)PostWithImage.Image.Bitmap.Width;
                PostWithImage.Height = (uint)PostWithImage.Image.Bitmap.Height;
                PostWithImage.ImageHash = PostWithImage.Image.CalculateImageHash();
                PostWithImage.ID = (uint)_DB.ExecuteInsert("posts", PostWithImage.ToDictionary(false));
                //Maybe Width + Height checks?
                PostWithImage.Image.Save(Path.Combine(ImageFolder, "image" + PostWithImage.ID));
                using (BooruImage thumbImage = PostWithImage.Image.CreateThumbnail(BooruInfo.ThumbnailSize, false))
                    thumbImage.Save(Path.Combine(ThumbFolder, "thumb" + PostWithImage.ID), BooruInfo.ThumbnailQuality);
                AddPostTags(PostWithImage);
                return PostWithImage.ID;
            }
            else throw new Exception("No permission");
        }

        public void EditPost(BooruUser User, BooruPost Post)
        {
            if (User.CanEditPosts)
            {
                if (_DB.ExecuteScalar<int>(SQLStatements.GetPostCountByID, Post.ID) > 0)
                {
                    if (!User.AdvancePostControl)
                    {
                        DataRow postRow = _DB.ExecuteRow(SQLStatements.GetPostByID, Post.ID);
                        BooruPost oldPost = BooruPost.FromRow(postRow);
                        Post.EditCount = oldPost.EditCount + 1;
                        Post.CreationDate = oldPost.CreationDate;
                        Post.User = oldPost.User;
                        Post.ViewCount = oldPost.ViewCount;
                    }
                    _DB.ExecuteNonQuery(SQLStatements.DeletePostByID, Post.ID);
                    _DB.ExecuteNonQuery(SQLStatements.DeletePostTagsByPostID, Post.ID);
                    AddPostTags(Post);
                    _DB.ExecuteInsert("posts", Post.ToDictionary(true));
                }
                else throw new Exception("Not found");
            }
            else throw new Exception("No permission");
        }

        public List<string> GetAllTags()
        {
            DataTable tagTable = _DB.ExecuteTable(SQLStatements.GetTags);
            BooruTagList allTags = BooruTagList.FromTable(tagTable);
            DataTable aliasTable = _DB.ExecuteTable(SQLStatements.GetAliases);
            List<string> tags = new List<string>(allTags.Count + aliasTable.Rows.Count);
            foreach (BooruTag tag in allTags)
                tags.Add(tag.Tag);
            foreach (DataRow aRow in aliasTable.Rows)
                tags.Add(Convert.ToString(aRow["alias"]));
            return tags;
        }

        public void EditTag(BooruUser User, BooruTag Tag)
        {
            if (User.CanEditTags)
            {
                if (_DB.ExecuteScalar<int>(SQLStatements.GetTagCountByID, Tag.ID) > 0)
                {
                    DataRow typeRow = _DB.ExecuteRow(SQLStatements.GetTagTypeByTypeName, Tag.Type);
                    if (typeRow != null)
                    {
                        uint typeID = Convert.ToUInt32(typeRow["id"]);
                        _DB.ExecuteNonQuery(SQLStatements.DeleteTagByID, Tag.ID);
                        _DB.ExecuteNonQuery(SQLStatements.InsertTagWithID, Tag.ID, Tag.Tag, typeID);
                    }
                    else throw new Exception("Not found");
                }
                else throw new Exception("Not found");
            }
            else throw new Exception("No permission");
        }

        public List<ulong> SearchImg(BooruUser User, byte[] Hash)
        {
            DataTable dupeTable = _DB.ExecuteTable(SQLStatements.GetDuplicatePosts, Convert.ToBase64String(Hash), User.MaxRating, 20);
            BooruPostList dupes = BooruPostList.FromTable(dupeTable);
            List<ulong> ids = new List<ulong>();
            foreach (BooruPost dupe in dupes)
                if (IsPrivacyAllowed(dupe, User))
                    ids.Add(dupe.ID);
            return ids;
        }

        public BooruUser Login(BooruUser User, string Username, string Password)
        {
            Password = Helper.ByteToString(Helper.MD5OfString(Password));
            DataRow userRow = _DB.ExecuteRow(SQLStatements.GetUserByUsername, Username);
            BooruUser user = BooruUser.FromRow(userRow);
            if (user != null)
                if (user.MD5Password == Password || (User == null ? false : User.IsAdmin))
                {
                    user.MD5Password = null;
                    return user;
                }
            throw new Exception("Login failed");
        }

        private bool IsPrivacyAllowed(BooruPost Post, BooruUser User)
        {
            if (Post.Private && !User.IsAdmin)
                return Post.User == User.Username;
            else return true;
        }

        private void AddPostTags(BooruPost newPost)
        {
            foreach (BooruTag tag in newPost.Tags)
            {
                DataRow existingTagRow = _DB.ExecuteRow(SQLStatements.GetTagByTagString, tag.Tag);
                BooruTag existingTag = BooruTag.FromRow(existingTagRow);
                if (existingTag == null)
                {
                    bool taggedAliasAdded = false;
                    DataRow existingAliasRow = _DB.ExecuteRow(SQLStatements.GetAliasByString, tag.Tag);
                    if (existingAliasRow != null)
                    {
                        ulong tagID = Convert.ToUInt64(existingAliasRow["tagid"]);
                        if (_DB.ExecuteScalar<int>(SQLStatements.GetTagCountByID, tagID) > 0)
                        {
                            _DB.ExecuteNonQuery(SQLStatements.InsertPostTag, newPost.ID, tagID);
                            taggedAliasAdded = true;
                        }
                    }
                    if (!taggedAliasAdded)
                    {
                        _DB.ExecuteNonQuery(SQLStatements.InsertTag, tag.Tag, BooruInfo.DefaultTagType);
                        ulong newTagID = _DB.GetLastInsertedID();
                        _DB.ExecuteNonQuery(SQLStatements.InsertPostTag, newPost.ID, newTagID);
                    }
                }
                else _DB.ExecuteNonQuery(SQLStatements.InsertPostTag, newPost.ID, existingTag.ID);
            }
        }
    }
}