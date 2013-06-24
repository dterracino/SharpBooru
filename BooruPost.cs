using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpBooru
{
    public class BooruPost : IDisposable
    {
        private int ID;
        private BooruImage Thumbnail;
        private BooruImage Image;
    }
}
