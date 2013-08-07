using System;

namespace TEAM_ALPHA.SQLBooru.Server.VirtualFileSystem
{
    public class VFSDelegateFile : VFSFile
    {
        private Action<Context> _Delegate;
        private bool _UseHelperHTMLWrapping;
        private string _Title;
        private string _MimeType;

        public bool UseHelperHTMLWrapping { get { return _UseHelperHTMLWrapping; } }
        public string Title { get { return _Title; } set { _Title = value; } }
        public string MimeType { get { return _MimeType; } set { _MimeType = value; } }

        public VFSDelegateFile(string Name, string MimeType, Action<Context> Delegate, bool UseHelperHTMLWrapping, string Title = null)
        {
            this.Name = Name;
            _Delegate = Delegate;
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
            _Delegate(Context);
            if (UseHelperHTMLWrapping)
                ServerHelper.WriteFooter(Context);
        }
    }
}
