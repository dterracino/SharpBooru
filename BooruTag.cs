using System;
using System.Drawing;

namespace TEAM_ALPHA.SharpBooru
{
    [Serializable]
    public class BooruTag
    {
        public BooruTag(string Tag) { this.Tag = Tag; }
        public BooruTag(string Tag, string Type, string Description, Color Color)
            : this(Tag)
        {
            this.Type = Type;
            this.Description = Description;
            this.Color = Color;
        }

        public string Tag = "unknown";
        public string Type = "Temporary";
        public string Description = "Temporary tags";
        public Color Color = Color.Black;
    }
}
