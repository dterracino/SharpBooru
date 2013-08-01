using System;
using System.Windows.Forms;

namespace TA.SharpBooru.Client.GUI.Controls
{
    public partial class EditPanel : UserControl
    {
        private Booru _Booru;
        private BooruPost _Post;

        public EditPanel()
        {
            InitializeComponent();
            this.Visible = false;
        }

        public void SetPost(Booru Booru, BooruPost Post)
        {
            _Booru = Booru;
            _Post = Post;
            textBoxTags.SetTags(Booru.GetAllTags());
            textBoxTags.Text = Post.Tags.ToString();
            textBoxComment.Text = Post.Comment;
            textBoxSource.Text = Post.Source;
            textBoxRating.Value = Post.Rating;
            checkBoxPrivate.Checked = Post.Private;
            this.Visible = true;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _Post.Tags = BooruTagList.FromString(textBoxTags.Text);
            _Post.Comment = textBoxComment.Text;
            _Post.Source = textBoxSource.Text;
            _Post.Rating = Convert.ToByte(textBoxRating.Value);
            _Post.Private = checkBoxPrivate.Checked;
            _Booru.SavePost(_Post);
            this.Visible = false;
        }

        public void CancelEdit()
        {
            this.Visible = false;
        }
    }
}
