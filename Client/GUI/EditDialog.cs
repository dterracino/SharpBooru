using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TA.SharpBooru.Client.GUI
{
    public partial class EditDialog : Form
    {
        public EditDialog() { InitializeComponent(); }

        public DialogResult ShowDialog(BooruClient Booru, ref BooruPost Post)
        {
            textBoxTags.SetTags(Booru.GetAllTags());
            textBoxTags.Text = Post.Tags.ToString();
            textBoxComment.Text = Post.Description;
            textBoxSource.Text = Post.Source;
            textBoxRating.Value = Post.Rating;
            checkBoxPrivate.Checked = Post.Private;
            textBoxOwner.Enabled = Booru.CurrentUser.AdvancePostControl;
            textBoxOwner.Text = Post.User;
            DialogResult dResult = this.ShowDialog();
            if (dResult == DialogResult.OK)
            {
                Post.Tags = BooruTagList.FromString(textBoxTags.Text);
                Post.Description = textBoxComment.Text;
                Post.Source = textBoxSource.Text;
                Post.Rating = Convert.ToByte(textBoxRating.Value);
                Post.Private = checkBoxPrivate.Checked;
                Post.User = textBoxOwner.Text;
            }
            return dResult;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
