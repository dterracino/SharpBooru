using System;
using System.Collections.Generic;

namespace TA.SharpBooru.Client.WebServer.VFS
{
    public class VFSBooruSearchFile : VFSFile
    {
        private string _Title = "Search";

        public string Title
        {
            get { return _Title; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    _Title = value;
            }
        }

        public VFSBooruSearchFile(string Name, string Title)
        {
            this.Name = Name;
            _Title = Title;
        }

        public override void Execute(Context Context)
        {
            ServerHelper.WriteHeader(Context, Title);
            int thumbsPerPage = 60; //TODO ThumbsPerPage property
            //int thumbsPerPage = Context.Booru.GetProperty<int>(Booru.Property.ServerThumbsPerPage);
            string tagSearch = string.Empty;
            if (Context.Params.GET.IsSet<string>("tags"))
                tagSearch = Context.Params.GET.Get<string>("tags");
            List<ulong> postIDs = Context.Booru.Search(tagSearch);
            int totalPages = (int)Math.Truncate((postIDs.Count - 1f) / thumbsPerPage) + 1;
            int page = 0;
            if (Context.Params.GET.IsSet<int>("page"))
                page = Context.Params.GET.Get<int>("page") - 1;
            if (page > totalPages - 1)
                page = totalPages - 1;
            else if (page < 0)
                page = 0;
            int count = postIDs.Count - page * thumbsPerPage;
            if (count > thumbsPerPage)
                count = thumbsPerPage;
            postIDs = postIDs.GetRange(page * thumbsPerPage, count);
            ServerHelper.WriteTableHeader(Context);
            ServerHelper.WriteSearchTextBox(Context, Name, tagSearch);
            /*
            Context.OutWriter.Write("<br>");
            Dictionary<BooruTag, int> top20dict = BooruHelper.GetTop20Tags(Context.Booru, posts);
            if (top20dict.Count > 0)
            {
                ServerHelper.WriteSubSectionHeader(Context, "Top 20 tags");
                foreach (KeyValuePair<BooruTag, int> topTag in BooruHelper.GetTop20Tags(Context.Booru, posts))
                {
                    string color = ColorTranslator.ToHtml(topTag.Key.Color);
                    Context.OutWriter.Write("{1} <span style=\"color:{0}\">", color, topTag.Value);
                    string tag = topTag.Key.Tag;
                    Context.OutWriter.Write("<a href=\"index?tags={0}\">", HttpUtility.HtmlEncode(tag));
                    Context.OutWriter.Write("{0}</a></span><br>", HttpUtility.HtmlEncode(tag));
                }
                ServerHelper.WriteSubSectionFooter(Context);
            }
            */
            ServerHelper.WriteTableMiddle(Context);
            if (postIDs.Count > 0)
            {
                Context.OutWriter.Write("<div class=\"wrap\">");
                foreach (ulong postID in postIDs)
                {
                    Context.OutWriter.Write("<a href=\"post?id={0}&amp;tags={1}\">", postID, tagSearch);
                    Context.OutWriter.Write("<img class=\"thumb\" alt=\"\" src=\"thumb?id={0}\"></a>", postID);
                }
                Context.OutWriter.Write("</div><br><div class=\"page_chooser\">");
                if (totalPages > 0)
                    if (page > 0)
                        Context.OutWriter.Write("<span class=\"pc_page\"><span class=\"pc_arrow\"><a href=\"{1}?page={0}&amp;tags={2}\">&#x21ab;</a></span></span>", page, Name, tagSearch);
                for (int i = 0; i < totalPages; i++)
                {
                    Context.OutWriter.Write("<span class=\"pc_page\">");
                    if (i != page)
                        Context.OutWriter.Write("<a href=\"{1}?page={0}&amp;tags={2}\">{0}</a>", i + 1, Name, tagSearch);
                    else Context.OutWriter.Write("<span class=\"pc_selected\">{0}</span>", i + 1);
                    Context.OutWriter.Write("</span>");
                }
                if (totalPages > 0)
                    if (page < totalPages - 1)
                        Context.OutWriter.Write("<span class=\"pc_page\"><span class=\"pc_arrow\"><a href=\"{1}?page={0}&amp;tags={2}\">&#x21ac;</a></span></span>", page + 2, Name, tagSearch);
                Context.OutWriter.Write("</div>");
            }
            else Context.OutWriter.Write("Nobody here but us chickens! - Why chickens?!");
            ServerHelper.WriteTableFooter(Context);
            ServerHelper.WriteFooter(Context);
        }
    }
}