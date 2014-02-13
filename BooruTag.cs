using System;
using System.Data;
using System.Drawing;
using System.Collections.Generic;

namespace TA.SharpBooru
{
    public class BooruTag : BooruResource, ICloneable
    {
        public ulong ID;

        public string Tag = "unknown";
        public string Type = "Temporary";
        public string Description = "Temporary tags";
        public Color Color = Color.Black;

        public BooruTag(string Tag) { this.Tag = Tag; }
        public BooruTag(string Tag, string Type, string Description, Color Color)
            : this(Tag)
        {
            this.Type = Type;
            this.Description = Description;
            this.Color = Color;
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

        public override void ToWriter(ReaderWriter Writer)
        {
            Writer.Write(ID);
            Writer.Write(Tag, true);
            Writer.Write(Type, true);
            Writer.Write(Description, true);
            Writer.Write(Color.ToArgb());
        }

        public static BooruTag FromReader(ReaderWriter Reader)
        {
            ulong id = Reader.ReadULong();
            return new BooruTag(Reader.ReadString())
            {
                ID = id,
                Type = Reader.ReadString(),
                Description = Reader.ReadString(),
                Color = Color.FromArgb(Reader.ReadInt())
            };
        }

        public static BooruTag FromRow(DataRow Row)
        {
            if (Row != null)
                return new BooruTag(Convert.ToString(Row["tag"]))
                {
                    ID = Convert.ToUInt64(Row["id"]),
                    Type = Convert.ToString(Row["type"]),
                    Description = Convert.ToString(Row["description"]),
                    Color = ColorTranslator.FromHtml(Convert.ToString(Row["color"]))
                };
            else return null;
        }

        public object Clone() { return MemberwiseClone(); }
    }

    public class BooruTagList : List<BooruTag>, ICloneable
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

        public void ToWriter(ReaderWriter Writer)
        {
            Writer.Write((uint)this.Count);
            this.ForEach(x => x.ToWriter(Writer));
        }

        public static BooruTagList FromReader(ReaderWriter Reader)
        {
            uint count = Reader.ReadUInt();
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

        public static BooruTagList FromTable(DataTable Table)
        {
            BooruTagList bTagList = new BooruTagList();
            foreach (DataRow row in Table.Rows)
                bTagList.Add(BooruTag.FromRow(row));
            return bTagList;
        }

        public List<string> ToStringList()
        {
            var strTags = new List<string>();
            ForEach(x => strTags.Add(x.Tag));
            return strTags;
        }

        public override string ToString() { return string.Join(" ", ToStringList()); }

        public object Clone()
        {
            BooruTagList cList = new BooruTagList();
            this.ForEach(x => cList.Add(x.Clone() as BooruTag));
            return cList;
        }
    }
}