using System;

namespace TA.SharpBooru.BooruAPIs
{
    public class BooruAPIPost : BooruPost
    {
        public string ImageURL = string.Empty;
        public string SampleURL = string.Empty;
        public string ThumbnailURL = string.Empty;
        public string SourceURL = string.Empty;
        public string APIName = string.Empty;

        public void DownloadImage()
        {
            if (_Image != null)
                _Image = BooruImage.FromURL(ImageURL);
        }

        private BooruImage _Image;
        public BooruImage Image
        {
            get
            {
                DownloadImage();
                return _Image;
            }
        }
    }
}