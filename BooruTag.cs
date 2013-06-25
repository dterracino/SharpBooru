using System;
using System.Drawing;

namespace TEAM_ALPHA.SharpBooru
{
    [Serializable]
    public class BooruTag
    {
        public string Tag = "unknown";
        public string Type;
        public string Description;
        public Color Color;
    }
}
