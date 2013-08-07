using System;
using System.Drawing;
using System.Collections.Generic;
using TEAM_ALPHA.SQLBooru.BooruAPIs;

namespace TEAM_ALPHA.SQLBooru.Server.VirtualFileSystem
{
    public class VFSBooruUploadFile : VFSFile
    {
        public VFSBooruUploadFile(string Name) { this.Name = Name; }

        public override void Execute(Context Context)
        {
            if (Context.User.Perm_Upload)
            {
                if (Context.Params.POST.IsSet<bool>("useapi"))
                {
                    if (Context.Params.POST.Get<bool>("useapi"))
                    {
                        string url = Context.Params.POST.Get<string>("url");
                        List<BooruAPIPost> api_posts = BooruAPI.SearchPostsPerURL(url);
                        api_posts.ForEach(x =>
                            {
                                try
                                {
                                    BooruPost post = new BooruPost(Context.Booru);
                                    string imgPath = Helper.DownloadTemporary(x.ImageURL);
                                    post.Image = new BooruImage(imgPath);
                                    post.Tags = BooruHelper.GetBooruTags(Context.Booru, x.Tags);
                                    post.Source = x.SourceURL;
                                    post.Comment = string.Format("Imported online from {0}", x.APIName);
                                    post.Rating = Context.Booru.GetProperty<byte>(Booru.Property.StandardRating);
                                }
                                catch
                                {
                                    Context.HTTPCode = 500;
                                    return;
                                }
                            });
                    }
                    else if (Context.Params.Files.Count > 0)
                        if (!string.IsNullOrWhiteSpace(Context.Params.Files[0].FileName))
                        {
                            BooruPost post = new BooruPost(Context.Booru);
                            try
                            {
                                Context.POSTFile postFile = Context.Params.Files[0];
                                BooruImage bImg = new BooruImage(postFile.Data);
                                post.Image = bImg;
                                post.Tags = BooruHelper.GetBooruTags(Context.Booru, Context.Params.POST.Get<string>("tags"));
                                if (Context.Params.POST.IsSet<string>("source"))
                                    post.Source = Context.Params.POST.Get<string>("source");
                                else post.Source = "WWW Upload";
                                if (Context.Params.POST.IsSet<string>("comment"))
                                    post.Comment = Context.Params.POST.Get<string>("comment");
                                if (Context.Params.POST.IsSet<byte>("rating"))
                                    post.Rating = Context.Params.POST.Get<byte>("rating");
                                else post.Rating = Context.Booru.GetProperty<byte>(Booru.Property.StandardRating);
                                post.User = Context.User.U_Username;
                                Context.InnerContext.Response.RedirectLocation = string.Format("/post?id={0}", post.ID);
                                Context.HTTPCode = 302;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.StackTrace);
                                post.Delete();
                                Context.HTTPCode = 500;
                            }
                            return;
                        }
                }
                ServerHelper.WriteHeader(Context, "Upload");
                Context.OutWriter.Write("<form action=\"\" method=\"POST\" enctype=\"multipart/form-data\"><table>");
                Context.OutWriter.Write("<tr><td>File</td><td><input style=\"width: 600px\" type=\"file\" name=\"file\" accept=\"image/*\"></td></tr>");
                Context.OutWriter.Write("<tr><td>Source</td><td><input style=\"width: 600px\" type=\"text\" name=\"source\"></td></tr>");
                Context.OutWriter.Write("<tr><td>Comment</td><td><input style=\"width: 600px\" type=\"text\" name=\"comment\"></td></tr>");
                Context.OutWriter.Write("<tr><td>Rating</td><td><input style=\"width: 600px\" type=\"text\" name=\"rating\"></td></tr>");
                Context.OutWriter.Write("<tr><td>Tags</td><td><textarea style=\"width: 600px\" rows=\"10\" name=\"tags\"></textarea></td></tr>");
                Context.OutWriter.Write("<tr><td></td><td><input type=\"submit\" value=\"OK\"></td></tr></table>");
                Context.OutWriter.Write("<input type=\"hidden\" name=\"useapi\" value=\"False\">");
                Context.OutWriter.Write("</form><br><br>");
                Context.OutWriter.Write("<form action=\"\" method=\"POST\">");
                Context.OutWriter.Write("Post link <input style=\"width: 800px\" name=\"url\" type=\"text\">");
                Context.OutWriter.Write("<input type=\"hidden\" name=\"useapi\" value=\"True\">");
                Context.OutWriter.Write("<input type=\"submit\" value=\"OK\">");
                Context.OutWriter.Write("</form>");
                ServerHelper.WriteFooter(Context);
            }
            else Context.HTTPCode = 403;
        }
    }
}
