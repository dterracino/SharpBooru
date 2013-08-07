using System;
using System.Drawing;
using System.Collections.Generic;

namespace TEAM_ALPHA.SQLBooru.Server.VirtualFileSystem
{
    public class VFSBooruImageFile : VFSFile
    {
        private bool _MainImage;

        public VFSBooruImageFile(string Name, bool MainImage)
        {
            this.Name = Name;
            _MainImage = MainImage;
        }

        public override void Execute(Context Context)
        {
            if (Context.Params.GET.IsSet<int>("id"))
            {
                int id = Context.Params.GET.Get<int>("id");
                using (BooruPost post = new BooruPost(Context.Booru, id))
                {
                    if (!string.IsNullOrWhiteSpace(Context.User.Perm_SearchPrefix) && post.User != Context.User.U_Username)
                    {
                        List<BooruPost> allowedPosts = Context.Booru.Search(Context.User.Perm_SearchPrefix);
                        if (!allowedPosts.Contains(post))
                        {
                            Context.HTTPCode = 403;
                            return;
                        }
                    }
                    string md5 = null;
                    if (_MainImage)
                    {
                        md5 = BooruHelper.GetImageMD5(Context.Booru, post.ImageID);
                        if (string.IsNullOrWhiteSpace(md5))
                            md5 = null;
                        string ifNoneMatch = Context.InnerContext.Request.Headers["If-None-Match"];
                        if (ifNoneMatch != null && md5 != null)
                            if (md5 == ifNoneMatch.ToLower())
                            {
                                Context.HTTPCode = 304;
                                return;
                            }
                    }
                    BooruImage bImg = _MainImage ? post.Image : post.Thumbnail;
                    if (bImg != null)
                    {
                        Context.MimeType = bImg.MimeType;
                        if (_MainImage)
                        {
                            if (md5 == null)
                            {
                                md5 = ServerHelper.MD5(bImg.Bytes);
                                BooruHelper.SetImageMD5(Context.Booru, post.ImageID, md5);
                            }
                            Context.InnerContext.Response.AddHeader("ETag", md5);
                        }
                        Context.InnerContext.Response.OutputStream.Write(bImg.Bytes, 0, bImg.Bytes.Length);
                        return;
                    }
                }
            }
            Context.HTTPCode = 404;
        }
    }
}