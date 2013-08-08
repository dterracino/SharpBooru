namespace TA.SharpBooru.Client.WebServer.VFS
{
    public class VFSBooruImageFile : VFSFile
    {
        private bool _MainImage;

        public VFSBooruImageFile(string Name, bool MainImage)
        {
            this.Name = Name;
            _MainImage = MainImage;
        }

        public override void Execute(Context Context)
        {
            if (Context.Params.GET.IsSet<int>("id"))
            {
                ulong id = Context.Params.GET.Get<ulong>("id");
                using (BooruImage img = _MainImage ? Context.Booru.GetImage(id) : Context.Booru.GetThumbnail(id))
                    Context.InnerContext.Response.OutputStream.Write(img.Bytes, 0, img.Bytes.Length);
                //TODO Implement If-None-Match and ETag
                //string ifNoneMatch = Context.InnerContext.Request.Headers["If-None-Match"];
                //Context.InnerContext.Response.AddHeader("ETag", md5);
            }
        }
    }
}