using System.IO;

namespace TA.SharpBooru.Server.WebServer.VFS
{
    public class VFSByteFile : VFSFile
    {
        private string _MimeType;
        private byte[] _Bytes;

        public byte[] Bytes { get { return _Bytes; } }
        public string MimeType { get { return _MimeType; } set { _MimeType = value; } }

        public VFSByteFile(string Name, string MimeType, byte[] Bytes)
        {
            this.Name = Name;
            _MimeType = MimeType;
            _Bytes = Bytes;
        }

        public VFSByteFile(string Name, string MimeType, Stream Stream)
        {
            this.Name = Name;
            _MimeType = MimeType;
            _Bytes = new byte[Stream.Length];
            Stream.Read(_Bytes, 0, _Bytes.Length);
        }

        public override void Execute(Context Context)
        {
            if (MimeType != null)
                Context.MimeType = MimeType;
            Context.InnerContext.Response.OutputStream.Write(_Bytes, 0, _Bytes.Length);
        }
    }
}