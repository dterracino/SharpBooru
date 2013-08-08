using System.Reflection;

namespace TA.SharpBooru.Client.WebServer.VFS
{
    public class VFSBooruInfoFile : VFSFile
    {
        private string _Title = "Booru Info";

        public string Title
        {
            get { return _Title; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    _Title = value;
            }
        }

        public VFSBooruInfoFile(string Name) { this.Name = Name; }

        public override void Execute(Context Context)
        {
            ServerHelper.WriteHeader(Context, _Title);
            Context.OutWriter.Write("<table>");
            string formatString = "<tr><td>{0}</td><td>{1}</td></tr>";
            Context.OutWriter.Write(formatString, "ClientIP", Context.ClientIP);
            Context.OutWriter.Write(formatString, "UserAgent", Context.UserAgent);
            Context.OutWriter.Write(formatString, "BooruName", Context.Booru.GetProperty<string>(Booru.Property.ServerBooruName));
            Context.OutWriter.Write(formatString, "DatabaseVersion", Context.Booru.GetProperty<int>(Booru.SystemProperty.DatabaseVersion));
            Context.OutWriter.Write(formatString, "ServerVersion", Assembly.GetExecutingAssembly().GetName().Version);
            Context.OutWriter.Write(formatString, "PostCount", Context.Booru.Search().Count);
            Context.OutWriter.Write(formatString, "TagCount", Context.Booru.SearchTags().Count);
            Context.OutWriter.Write(formatString, "SearchPrefix", Context.User.Perm_SearchPrefix);
            Context.OutWriter.Write(formatString, "ThumbsSize", Context.Booru.GetProperty<int>(Booru.Property.ServerThumbsSize));
            Context.OutWriter.Write(formatString, "ThumbsPerPage", Context.Booru.GetProperty<int>(Booru.Property.ServerThumbsPerPage));
            Context.OutWriter.Write("</table>");
            ServerHelper.WriteFooter(Context);
        }
    }
}
