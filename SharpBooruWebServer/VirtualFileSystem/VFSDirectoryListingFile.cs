using System;

namespace TA.SharpBooru.Client.WebServer.VFS
{
    public class VFSDirectoryListingFile : VFSFile
    {
        private VFSDirectory _Directory;
        private bool _UseHelperHTMLWrapping;
        private string _CustomTitle;

        public VFSDirectory Directory { get { return _Directory; } }
        public bool UseHelperHTMLWrapping { get { return _UseHelperHTMLWrapping; } }
        public string CustomTitle { get { return _CustomTitle; } set { _CustomTitle = value; } }

        public VFSDirectoryListingFile(VFSDirectory Directory, bool UseHelperHTMLWrapping, string CustomTitle = null)
        {
            _Directory = Directory;
            _UseHelperHTMLWrapping = UseHelperHTMLWrapping;
            _CustomTitle = null;
        }

        public override void Execute(Context Context)
        {
            string indexOfString = "Index";
            if (!string.IsNullOrWhiteSpace(_Directory.Name))
                indexOfString += " of " + _Directory.Name;
            if (UseHelperHTMLWrapping)
                ServerHelper.WriteHeader(Context, CustomTitle ?? indexOfString);
            Context.OutWriter.WriteLine("<b>{0}</b><br><br>", indexOfString);
            foreach (VFSEntry entry in _Directory.Entrys)
            {
                Context.OutWriter.WriteLine("<a href=\"{0}/{1}\">", _Directory.Name, entry.Name);
                Context.OutWriter.WriteLine("{0}{1}", entry.Name, entry is VFSDirectory ? "/" : string.Empty);
                Context.OutWriter.WriteLine("</a><br>");
            }
            if (UseHelperHTMLWrapping)
                ServerHelper.WriteFooter(Context);
        }
    }
}
