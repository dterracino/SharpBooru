using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

namespace TA.SharpBooru
{
    public class BooruTag
    {
        public ulong ID;

        public string Tag = "unknown";
        public string Type = "Temporary";
        public string Description = "Temporary tags";
        public int Color = unchecked((int)0xFF000000);

        public BooruTag(string Tag) { this.Tag = Tag; }
        public BooruTag(string Tag, string Type, string Description, Color Color)
            : this(Tag)
        {
            this.Type = Type;
            this.Description = Description;
            this.Color = Color.ToArgb();
        }

        public static bool operator ==(BooruTag Tag1, BooruTag Tag2)
        {
            if ((object)Tag1 == null && (object)Tag2 == null)
                return true;
            else if ((object)Tag1 == null || (object)Tag2 == null)
                return false;
            else return Tag1.ID == Tag2.ID; 
        }

        public static bool operator !=(BooruTag Tag1, BooruTag Tag2) { return !(Tag1 == Tag2); }

        public override bool Equals(object obj) { return this == obj as BooruTag; }

        public override int GetHashCode() { return ID.GetHashCode(); }

        public void ToWriter(BinaryWriter Writer, bool OnlyID = false)
        {
            Writer.Write(ID);
            if (!OnlyID)
            {
                Writer.Write(Tag);
                Writer.Write(Type);
                Writer.Write(Description);
                Writer.Write(Color);
            }
        }

        public static BooruTag FromReader(BinaryReader Reader)
        {
            ulong id = Reader.ReadUInt64();
            return new BooruTag(Reader.ReadString())
            {
                ID = id,
                Type = Reader.ReadString(),
                Description = Reader.ReadString(),
                Color = Reader.ReadInt32()
            };
        }
    }

    public class BooruTagList : List<BooruTag>
    {
        public BooruTag this[ulong ID]
        {
            get
            {
                foreach (BooruTag tag in this)
                    if (tag.ID == ID)
                        return tag;
                return null;
            }
        }

        public BooruTag this[string Tag]
        {
            get
            {
                foreach (BooruTag tag in this)
                    if (tag.Tag == Tag)
                        return tag;
                return null;
            }
        }

        public new bool Contains(BooruTag Tag) { return Contains(Tag.Tag); }
        public bool Contains(ulong ID) { return this[ID] != null; }
        public bool Contains(string Tag)
        {
            foreach (BooruTag tag in this)
                if (tag.Tag == Tag)
                    return true;
            return false;
        }


        public int Remove(ulong ID) { return this.RemoveAll(x => { return x.ID == ID; }); }

        public void ToWriter(BinaryWriter Writer)
        {
            Writer.Write((uint)this.Count);
            this.ForEach(x => x.ToWriter(Writer));
        }

        public static BooruTagList FromReader(BinaryReader Reader)
        {
            uint count = Reader.ReadUInt32();
            BooruTagList bTagList = new BooruTagList();
            for (uint i = 0; i < count; i++)
                bTagList.Add(BooruTag.FromReader(Reader));
            return bTagList;
        }

        public static BooruTagList FromString(string Tags)
        {
            BooruTagList bTagList = new BooruTagList();
            if (!string.IsNullOrWhiteSpace(Tags))
            {
                string[] parts = Tags.Split(new char[4] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts)
                    bTagList.Add(new BooruTag(part.ToLower()));
            }
            return bTagList;
        }

        public List<string> ToStringList()
        {
            var strTags = new List<string>();
            ForEach(x => strTags.Add(x.Tag));
            return strTags;
        }

        public override string ToString() { return string.Join(" ", ToStringList()); }
    }
}