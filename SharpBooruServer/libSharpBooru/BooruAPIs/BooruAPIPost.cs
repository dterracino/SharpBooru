namespace TA.SharpBooru.BooruAPIs
{
    public class BooruAPIPost : BooruPost
    {
        public string ImageURL = string.Empty;
        public string SampleURL = string.Empty;
        public string ThumbnailURL = string.Empty;
        public string SourceURL = string.Empty;
        public string APIName = string.Empty;

        public void DownloadImage() //TODO Progresscallback
        {
            if (Image == null)
                Image = BooruImage.FromURL(ImageURL);
        }
    }
}