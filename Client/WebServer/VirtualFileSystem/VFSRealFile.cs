using System;
using System.IO;

namespace TA.SharpBooru.Client.WebServer.VFS
{
    public class VFSRealFile : VFSFile
    {
        private string _File;
        private bool _UseHelperHTMLWrapping;
        private string _Title;
        private string _MimeType;

        public string File { get { return _File; } }
        public bool UseHelperHTMLWrapping { get { return _UseHelperHTMLWrapping; } }
        public string Title { get { return _Title; } set { _Title = value; } }
        public string MimeType { get { return _MimeType; } set { _MimeType = value; } }

        public VFSRealFile(string Name, string MimeType, string File, bool UseHelperHTMLWrapping, string Title = null)
        {
            this.Name = Name;
            if (File == null)
                throw new ArgumentNullException("File");
            else if (string.IsNullOrWhiteSpace(File))
                throw new ArgumentException("File");
            else _File = File;
            _UseHelperHTMLWrapping = UseHelperHTMLWrapping;
            _Title = Title;
            _MimeType = MimeType;
        }

        public override void Execute(Context Context)
        {
            if (MimeType != null)
                Context.MimeType = MimeType;
            if (UseHelperHTMLWrapping)
                WebserverHelper.WriteHeader(Context, Title ?? Name);
            if (System.IO.File.Exists(_File))
                using (FileStream fs = System.IO.File.Open(_File, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    byte[] buffer = new byte[1024];
                    while (true)
                    {
                        int read = fs.Read(buffer, 0, buffer.Length);
                        if (read > 0)
                            Context.InnerContext.Response.OutputStream.Write(buffer, 0, read);
                        else break;
                    }
                }
            else Context.HTTPCode = 404;
            if (UseHelperHTMLWrapping)
                WebserverHelper.WriteFooter(Context);
        }
    }
}
