using System;
using TA.SharpBooru.Client;

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
            if (_DownloadedImage != null)
                _DownloadedImage = BooruImage.FromURL(ImageURL);
        }

        private BooruImage _DownloadedImage;
        public BooruImage DownloadedImage
        {
            get
            {
                DownloadImage();
                return _DownloadedImage;
            }
        }
    }
}