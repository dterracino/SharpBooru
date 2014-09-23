using System;
using System.IO;
using System.Data;
using System.Collections.Generic;

namespace TA.SharpBooru
{
    public class ServerBooru : IDisposable
    {
        private SQLiteWrapper _DB;
        private BooruInfo _BooruInfo = null;
        private string _Folder;
        private ImageOptimizer _ImgOptimizer;

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

        public ServerBooru(string Folder)
        {
            _ImgOptimizer = new ImageOptimizer(null);
            string dbPath = Path.Combine(Folder, "booru.db");
            if (File.Exists(dbPath))
                if (Directory.Exists(Path.Combine(Folder, "images")))
                    if (Directory.Exists(Path.Combine(Folder, "thumbs")))
                    //if (Directory.Exists(Path.Combine(Folder, "avatars")))
                    {
                        _Folder = Folder;
                        _DB = new SQLiteWrapper(dbPath);
                        return;
                    }
            throw new Exception("No valid booru directory");
        }

        public void Dispose()
        {
            _DB.Dispose();
            _ImgOptimizer.Dispose();
        }

        public BooruPost GetPost(BooruUser User, ulong PostID, bool IncludeThumbnail)
        {
            DataRow postRow = _DB.ExecuteRow(SQLStatements.GetPostByID, PostID);
            BooruPost post = BooruPost.FromRow(postRow);
            if (post != null)
            {
                DataTable tagTable = _DB.ExecuteTable(SQLStatements.GetTagsByPostID, PostID);
                post.Tags = BooruTagList.FromTable(tagTable);
                if (User == null)
                    return post;
                else if (post.Rating <= User.MaxRating && IsPrivacyAllowed(post, User))
                    return post;
                else throw new BooruException(BooruException.ErrorCodes.NoPermission);
            }
            else throw new BooruException(BooruException.ErrorCodes.ResourceNotFound);
        }

        public BooruImage GetThumbnail(BooruUser User, ulong PostID)
        {
            DataRow postRow = _DB.ExecuteRow(SQLStatements.GetPostByID, PostID);
            BooruPost post = BooruPost.FromRow(postRow);
            if (post != null)
            {
                string path = Path.Combine(ThumbFolder, "thumb" + PostID);
                if (User == null)
                    return BooruImage.FromFile(path);
        else   if (post.Rating <= User.MaxRating && IsPrivacyAllowed(post, User))
                    return BooruImage.FromFile(path);
                else throw new BooruException(BooruException.ErrorCodes.NoPermission);
            }
            else throw new BooruException(BooruException.ErrorCodes.ResourceNotFound);
        }

        public BooruImage GetImage(BooruUser User, ulong PostID)
        {
            DataRow postRow = _DB.ExecuteRow(SQLStatements.GetPostByID, PostID);
            BooruPost post = BooruPost.FromRow(postRow);
            if (post != null)
            {
                if (User == null || (post.Rating <= User.MaxRating && IsPrivacyAllowed(post, User))) //                     TEST!!!
                {
                    BooruImage image = BooruImage.FromFile(Path.Combine(ImageFolder, "image" + PostID));
                    _DB.ExecuteNonQuery(SQLStatements.UpdateIncrementViewCount, PostID);
                    return image;
                }
                else throw new BooruException(BooruException.ErrorCodes.NoPermission);
            }
            else throw new BooruException(BooruException.ErrorCodes.ResourceNotFound);
        }

        public List<ulong> Search(BooruUser User, string SearchExpression)
        {
            BooruPostList searchedPosts = BooruSearch.DoSearch(SearchExpression, this);
            BooruPostList postsToSend = new BooruPostList();
            searchedPosts.ForEach(x =>
            {
                if (User == null)
                    postsToSend.Add(x);
                else if (x.Rating <= User.MaxRating && IsPrivacyAllowed(x, User))
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
                else throw new BooruException(BooruException.ErrorCodes.ResourceNotFound);
            }
            else throw new BooruException(BooruException.ErrorCodes.NoPermission);
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
                else throw new BooruException(BooruException.ErrorCodes.ResourceNotFound);
            }
            else throw new BooruException(BooruException.ErrorCodes.NoPermission);
        }

        public BooruTag GetTag(BooruUser User, string Tag)
        {
            DataRow tagRow = _DB.ExecuteRow(SQLStatements.GetTagByTagString, Tag);
            BooruTag tag = BooruTag.FromRow(tagRow);
            if (tag != null)
                return tag;
            else throw new BooruException(BooruException.ErrorCodes.ResourceNotFound);
        }

        public BooruTag GetTag(BooruUser User, ulong TagID)
        {
            DataRow tagRow = _DB.ExecuteRow(SQLStatements.GetTagByID, TagID);
            BooruTag tag = BooruTag.FromRow(tagRow);
            if (tag != null)
                return tag;
            else throw new BooruException(BooruException.ErrorCodes.ResourceNotFound);
        }

        public void AddAlias(BooruUser User, string Alias, ulong TagID)
        {
            if (User.CanEditTags) //TODO Alias permission?
            {
                if (_DB.ExecuteScalar<int>(SQLStatements.GetTagCountByID, TagID) > 0)
                    _DB.ExecuteNonQuery(SQLStatements.InsertAlias, Alias, TagID);
                else throw new BooruException(BooruException.ErrorCodes.ResourceNotFound);
            }
            else throw new BooruException(BooruException.ErrorCodes.NoPermission);
        }

        public void AddUser(BooruUser User, BooruUser NewUser)
        {
            if (User.IsAdmin)
                _DB.ExecuteInsert("users", NewUser.ToDictionary(false));
            else throw new BooruException(BooruException.ErrorCodes.NoPermission);
        }

        public void DeleteUser(BooruUser User, string Username)
        {
            if (User.IsAdmin)
                _DB.ExecuteNonQuery(SQLStatements.DeleteUserByUsername, Username);
            else throw new BooruException(BooruException.ErrorCodes.NoPermission);
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
                    string thumbPath = Path.Combine(ThumbFolder, "thumb" + PostID);
                    string imagePath = Path.Combine(ImageFolder, "image" + PostID);
                    using (BooruImage thumbImage = Image.CreateThumbnail(BooruInfo.ThumbnailSize, false))
                        thumbImage.Save(thumbPath, BooruInfo.ThumbnailQuality);
                    Image.Save(imagePath);
                    _ImgOptimizer.Optimize(thumbPath);
                    _ImgOptimizer.Optimize(imagePath);
                    post.Width = (uint)Image.Bitmap.Width;
                    post.Height = (uint)Image.Bitmap.Height;
                    post.ImageHash = Image.CalculateImageHash();
                    _DB.ExecuteNonQuery(SQLStatements.DeletePostByID, PostID);
                    _DB.ExecuteInsert("posts", post.ToDictionary(true));
                }
                else throw new BooruException(BooruException.ErrorCodes.ResourceNotFound);
            }
            else throw new BooruException(BooruException.ErrorCodes.NoPermission);
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
                string thumbPath = Path.Combine(ThumbFolder, "thumb" + PostWithImage.ID);
                string imagePath = Path.Combine(ImageFolder, "image" + PostWithImage.ID);
                using (BooruImage thumbImage = PostWithImage.Image.CreateThumbnail(BooruInfo.ThumbnailSize, false))
                    thumbImage.Save(thumbPath, BooruInfo.ThumbnailQuality);
                PostWithImage.Image.Save(imagePath);
                _ImgOptimizer.Optimize(thumbPath);
                _ImgOptimizer.Optimize(imagePath);
                AddPostTags(PostWithImage);
                return PostWithImage.ID;
            }
            else throw new BooruException(BooruException.ErrorCodes.NoPermission);
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
                else throw new BooruException(BooruException.ErrorCodes.ResourceNotFound);
            }
            else throw new BooruException(BooruException.ErrorCodes.NoPermission);
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
                    else throw new BooruException(BooruException.ErrorCodes.ResourceNotFound);
                }
                else throw new BooruException(BooruException.ErrorCodes.ResourceNotFound);
            }
            else throw new BooruException(BooruException.ErrorCodes.NoPermission);
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
            Password = Helper.BytesToString(Helper.MD5OfString(Password));
            DataRow userRow = _DB.ExecuteRow(SQLStatements.GetUserByUsername, Username);
            BooruUser user = BooruUser.FromRow(userRow);
            if (user != null)
                if (user.MD5Password == Password || (User == null ? false : User.IsAdmin))
                {
                    user.MD5Password = null;
                    return user;
                }
            throw new BooruException(BooruException.ErrorCodes.LoginFailed);
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