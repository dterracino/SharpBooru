using System;
using System.IO;
using System.Collections.Generic;

namespace TA.SharpBooru.Client.CLI
{
    public class BooruConsole : ConsoleEx
    {
        private Booru _Booru;

        public BooruConsole(Booru Booru)
            : base()
        { _Booru = Booru; }

        public override string Prompt { get { return "BOORU> "; } }

        protected override void PopulateCommandList(List<Command> Commands, TextWriter Out)
        {
            Commands.Add(new Command("post info", new Action<ulong>((id) =>
                {
                    BooruPost post = _Booru.GetPost(id, false);
                    Out.WriteLine("Post #{0} uploaded {1} by {2}:", post.ID, post.CreationDate, post.Owner);
                    Out.WriteLine("Viewed {0} times / Edited {1} times", post.ViewCount, post.EditCount);
                    Out.WriteLine("Rating {0}, Image size {1} x {2}", post.Rating, post.Width, post.Height);
                    Out.WriteLine("Source: {0}", post.Source);
                    Out.WriteLine("Post is {0}, Comment: {1}", post.Private ? "private" : "public", post.Comment);
                    Out.Write("Tags:"); post.Tags.ForEach(x => Console.Write(" {0}", x.Tag));
                    Out.WriteLine();
                })));
            Commands.Add(new Command("server kill", new Action(() => _Booru.ForceKillServer())));
            Commands.Add(new Command("server save", new Action(() => _Booru.SaveServerBooru())));
            Commands.Add(new Command("tag delete", new Action<ulong>((id) => _Booru.DeleteTag(id))));
            //tag edit
            //post edit
            //post add
            //post import
            //post delete
            //image get and open
            /*
            ulong id = Convert.ToUInt64(args[2]);
            using (BooruImage bImage = booru.GetImage(id))
                switch (args[1])
                {
                    case "get": bImage.Save(args[3]); break;
                    case "view":
                    case "open":
                        {
                            string tempFile = Helper.GetTempFile();
                            tempFile = string.Format("{0}.{1}", tempFile, bImage.ImageFormatExtension);
                            bImage.Save(tempFile);
                            System.Diagnostics.Process.Start(tempFile);
                        } break;
                }
            */
        }
    }
}