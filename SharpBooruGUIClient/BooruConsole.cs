using System;
using System.IO;

namespace TA.SharpBooru.Client.GUI
{
    public class BooruConsole : InteractiveConsole
    {
        public BooruConsole(Booru Booru)
            : this(Booru, null, null) { }

        public BooruConsole(Booru Booru, TextWriter Out, TextReader In)
            : base(null, Out, In)
        {
            this.Prompt = "BOORU> ";
            this.Commands.Add(new Command("post info", new Action<ulong>((id) =>
                {
                    BooruPost post = Booru.GetPost(id);
                    Out.WriteLine("Post #{0} uploaded {1} by {2}:", post.ID, post.CreationDate, post.Owner);
                    Out.WriteLine("Viewed {0} times / Edited {1} times", post.ViewCount, post.EditCount);
                    Out.WriteLine("Rating {0}, Image size {1} x {2}", post.Rating, post.Width, post.Height);
                    Out.WriteLine("Source: {0}", post.Source);
                    Out.WriteLine("Post is {0}, Comment: {1}", post.Private ? "private" : "public", post.Comment);
                    Out.Write("Tags:"); post.Tags.ForEach(x => Console.Write(" {0}", x.Tag));
                })));
        }
    }
}