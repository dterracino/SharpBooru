namespace TA.SharpBooru.Client.WebServer.VFS
{
    public class VFSErrorFile : VFSFile
    {
        private int _ErrorCode;
        private string _CustomTitle;

        public string CustomTitle { get { return _CustomTitle; } set { _CustomTitle = value; } }

        public VFSErrorFile(int ErrorCode, string CustomTitle = null)
        {
            Name = string.Format("error{0}", _ErrorCode);
            _ErrorCode = ErrorCode;
            _CustomTitle = CustomTitle;
        }

        public override void Execute(Context Context)
        {
            WebserverHelper.WriteMinimalHeader(Context, string.Format("ERROR {0}", _ErrorCode));
            Context.OutWriter.Write("<span style=\"font-family: arial\"><span style=\"font-size: 40px\">ERROR {0}</span><br>{1}", _ErrorCode, WebserverHelper.GetHTTPCodeDescription(_ErrorCode));
            WebserverHelper.WriteBackButton(Context, true, "Eat shit and die!");
            Context.OutWriter.Write("</span>");
            WebserverHelper.WriteMinimalFooter(Context);
        }
    }
}
