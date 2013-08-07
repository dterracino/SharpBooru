using System;
using System.Web;
using System.Drawing;
using System.Collections.Generic;

namespace TEAM_ALPHA.SQLBooru.Server.VirtualFileSystem
{
    public class VFSBooruPostFile : VFSFile
    {
        private string _Title = "Post #{0}";

        public string Title
        {
            get { return _Title; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    _Title = value;
            }
        }

        public VFSBooruPostFile(string Name, string Title)
        {
            this.Name = Name;
            _Title = Title;
        }

        public override void Execute(Context Context)
        {
            if (Context.Params.GET.IsSet<int>("id"))
            {
                int id = Context.Params.GET.Get<int>("id");
                bool editMode = true;
                if (Context.Params.POST.IsSet<bool>("edit_save"))
                    if (Context.Params.POST.Get<bool>("edit_save"))
                        try
                        {
                            BooruPost epost = new BooruPost(Context.Booru, id);
                            epost.Source = Context.Params.POST.Get<string>("source");
                            epost.Rating = Context.Params.POST.Get<byte>("rating");
                            epost.Tags = BooruHelper.GetBooruTags(Context.Booru, Context.Params.POST.Get<string>("tags"));
                            editMode = false;
                        }
                        catch
                        {
                            Context.HTTPCode = 500;
                            return;
                        }
                if (editMode)
                {
                    editMode = false;
                    if (Context.Params.GET.IsSet<bool>("edit"))
                        if (Context.Params.GET.Get<bool>("edit"))
                            if (!Context.User.Perm_Edit)
                            {
                                Context.HTTPCode = 403;
                                return;
                            }
                            else editMode = Context.Params.GET.Get<bool>("edit");
                }
                BooruPost post = null;
                if (id < 0 || !string.IsNullOrWhiteSpace(Context.User.Perm_SearchPrefix))
                {
                    List<BooruPost> allowedPosts = Context.Booru.Search(Context.User.Perm_SearchPrefix);
                    if (id < 0)
                    {
                        post = allowedPosts[(new Random()).Next(0, allowedPosts.Count)];
                        id = post.ID;
                    }
                    else post = new BooruPost(Context.Booru, id);
                    if (!string.IsNullOrWhiteSpace(Context.User.Perm_SearchPrefix) && post.User != Context.User.U_Username)
                    {
                        if (!allowedPosts.Contains(post))
                        {
                            Context.HTTPCode = 403;
                            return;
                        }
                    }
                }
                else post = new BooruPost(Context.Booru, id);
                ServerHelper.WriteHeader(Context, string.Format(Title, post.ID), Properties.Resources.imgscript_js);
                ServerHelper.WriteTableHeader(Context, "vtable");
                string tagSearch = null;
                if (Context.Params.GET.IsSet<string>("tags"))
                    tagSearch = Context.Params.GET.Get<string>("tags");
                ServerHelper.WriteSearchTextBox(Context, "index", tagSearch);
                Context.OutWriter.Write("<br>");
                List<BooruTag> tags = post.Tags;
                if (tags.Count > 0)
                {
                    ServerHelper.WriteSubSectionHeader(Context, "Tags");
                    tags.ForEach(x =>
                    {
                        string color = ColorTranslator.ToHtml(x.Color);
                        Context.OutWriter.Write("<span style=\"color:{0}\">", color);
                        string tag = x.Tag;
                        Context.OutWriter.Write("<a href=\"index?tags={0}\">", HttpUtility.HtmlEncode(tag));
                        Context.OutWriter.Write("{0}</a></span><br>", HttpUtility.HtmlEncode(tag));
                    });
                    ServerHelper.WriteSubSectionFooter(Context);
                }
                string source = post.Source;
                if (!string.IsNullOrWhiteSpace(source))
                {
                    ServerHelper.WriteSubSectionHeader(Context, "Source");
                    if (Helper.CheckURL(post.Source, false))
                        Context.OutWriter.Write("<a href=\"{0}\">{0}</a>", HttpUtility.HtmlEncode(post.Source));
                    else Context.OutWriter.Write(HttpUtility.HtmlEncode(post.Source));
                    ServerHelper.WriteSubSectionFooter(Context);
                }
                byte rating = post.Rating;
                ServerHelper.WriteSubSection(Context, "Rating", "{0}: {1}", rating, BooruHelper.GetRatingDescription(Context.Booru, rating));
                ServerHelper.WriteSubSection(Context, "Size", "{0}x{1}", post.Width, post.Height);
                ServerHelper.WriteSubSection(Context, "Counter", "Views: {0}<br>Edits: {1}", post.ViewCount, post.EditCount);
                ServerHelper.WriteTableMiddle(Context);
                Context.OutWriter.Write("<img id=\"mimg\" class=\"mimg\" alt=\"\" src=\"image?id={0}\"><br>", post.ID);
                if (editMode)
                {
                    Context.OutWriter.Write("<form action=\"\" method=\"POST\">");
                    Context.OutWriter.Write("<input type=\"hidden\" name=\"edit_save\" value=\"true\"><table>");
                    Context.OutWriter.Write("<tr><td>Source</td><td><input style=\"width: 600px\" type=\"text\" name=\"source\" value=\"{0}\"></td></tr>", post.Source);
                    Context.OutWriter.Write("<tr><td>Rating</td><td><input style=\"width: 600px\" type=\"text\" name=\"rating\" value=\"{0}\"></td></tr>", post.Rating);
                    List<string> tagStrings = new List<string>();
                    post.Tags.ForEach(x => tagStrings.Add(x.Tag));
                    Context.OutWriter.Write("<tr><td>Tags</td><td><textarea style=\"width: 600px\" rows=\"10\" name=\"tags\">{0}</textarea></td></tr>", string.Join("\n", tagStrings));
                    Context.OutWriter.Write("<tr><td></td><td style=\"text-align: right\"><input type=\"submit\" value=\"OK\"></td></tr>");
                    Context.OutWriter.Write("</table></form>");
                }
                else if (Context.User.Perm_Edit)
                {
                    Context.OutWriter.Write("<a href=\"?id={0}&amp;tags={1}&amp;edit=true\">Edit</a> | ", id, tagSearch);
                    Context.OutWriter.Write("<a href=\"?id={0}&amp;tags={1}&amp;edit=true\">Delete</a>", id, tagSearch);
                }
                ServerHelper.WriteTableFooter(Context);
                post.ViewCount++;
                ServerHelper.WriteFooter(Context);
            }
            else Context.HTTPCode = 404;
        }
    }
}