using System.Web;
using System.Drawing;
using System.Collections.Generic;

namespace TA.SharpBooru.Client.WebServer.VFS
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
                ulong id = Context.Params.GET.Get<ulong>("id");
                using (BooruPost post = Context.Booru.GetPost(id))
                {
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
                    ServerHelper.WriteSubSection(Context, "User", "{0}", post.User);
                    if (!string.IsNullOrWhiteSpace(post.Source))
                    {
                        ServerHelper.WriteSubSectionHeader(Context, "Source");
                        if (Helper.CheckURL(post.Source, false))
                            Context.OutWriter.Write("<a href=\"{0}\">{0}</a>", HttpUtility.HtmlEncode(post.Source));
                        else Context.OutWriter.Write(HttpUtility.HtmlEncode(post.Source));
                        ServerHelper.WriteSubSectionFooter(Context);
                    }
                    ServerHelper.WriteSubSection(Context, "Rating", "{0}", post.Rating);
                    ServerHelper.WriteSubSection(Context, "Size", "{0}x{1}", post.Width, post.Height);
                    ServerHelper.WriteSubSection(Context, "Counters", "Views: {0}<br>Edits: {1}", post.ViewCount, post.EditCount);
                    ServerHelper.WriteTableMiddle(Context);
                    Context.OutWriter.Write("<img id=\"mimg\" class=\"mimg\" alt=\"\" src=\"image?id={0}\">", post.ID);
                    /*
                    if (editMode)
                    {
                        Context.OutWriter.Write("<br><form action=\"\" method=\"POST\">");
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
                        Context.OutWriter.Write("<br><a href=\"?id={0}&amp;tags={1}&amp;edit=true\">Edit</a> | ", id, tagSearch);
                        Context.OutWriter.Write("<a href=\"?id={0}&amp;tags={1}&amp;edit=true\">Delete</a>", id, tagSearch);
                    }
                    */
                    ServerHelper.WriteTableFooter(Context);
                    //post.ViewCount++;
                    ServerHelper.WriteFooter(Context);
                }
            }
            else Context.HTTPCode = 404;
        }
    }
}