using System;
using System.Drawing;
using System.Windows.Forms;

namespace TA.SharpBooru.Client.GUI
{
    public static class GUIHelper
    {
        public static int SetListViewPadding(ListView ListView, int LeftPadding, int TopPadding) { return Helper.SetListViewPadding(ListView.Handle, LeftPadding, TopPadding); }

        public static ToolTip CreateToolTip(Control Control, string Text)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(Control, Text);
            return tt;
        }

        public static void SetFormCentered(Form Parent, Form Child)
        {
            Point childLocation = new Point(
                Parent.Location.X + (Parent.Width - Child.Width) / 2,
                Parent.Location.Y + (Parent.Height - Child.Height) / 2);
            Child.Load += (sender, e) => { Child.Location = childLocation; };
        }
    }
}