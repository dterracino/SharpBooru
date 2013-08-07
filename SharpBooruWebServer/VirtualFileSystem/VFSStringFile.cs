using System;

namespace TEAM_ALPHA.SQLBooru.Server.VirtualFileSystem
{
    public class VFSStringFile : VFSFile
    {
        private string _Content;
        private bool _UseHelperHTMLWrapping;
        private string _Title;
        private string _MimeType;

        public string Content { get { return _Content; } }
        public bool UseHelperHTMLWrapping { get { return _UseHelperHTMLWrapping; } }
        public string Title { get { return _Title; } set { _Title = value; } }
        public string MimeType { get { return _MimeType; } set { _MimeType = value; } }

        public VFSStringFile(string Name, string MimeType, string Content, bool UseHelperHTMLWrapping, string Title = null)
        {
            this.Name = Name;
            _Content = Content;
            _Title = Title;
            _UseHelperHTMLWrapping = UseHelperHTMLWrapping;
            _MimeType = MimeType;
        }

        public override void Execute(Context Context)
        {
            if (!string.IsNullOrWhiteSpace(MimeType))
                Context.MimeType = MimeType;
            if (UseHelperHTMLWrapping)
                ServerHelper.WriteHeader(Context, Title ?? "Text output");
            Context.OutWriter.Write(_Content);
            if (UseHelperHTMLWrapping)
                ServerHelper.WriteFooter(Context);
        }
    }
}