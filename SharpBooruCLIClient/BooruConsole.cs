using System;
using System.IO;
using System.Collections.Generic;
using TA.SharpBooru.BooruAPIs;

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
            Commands.Add(new Command("post info", "post info <ID>", new Action<ulong>((id) =>
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
            Commands.Add(new Command("server kill", "server kill", new Action(() => _Booru.ForceKillServer())));
            Commands.Add(new Command("server save", "server save", new Action(() => _Booru.SaveServerBooru())));
            Commands.Add(new Command("tag delete", "tag delete <ID>", new Action<ulong>((id) => _Booru.DeleteTag(id))));
            Commands.Add(new Command("user change", "user change <Username> [Password]", new Action<string, string>((un, pw) => _Booru.ChangeUser(un, pw))));
            //tag edit
            //post edit
            //post add
            Commands.Add(new Command("post import", "post import <URL> <Tags> <Private>", new Action<string, string, bool>((url, tags, privat) =>
                {
                    if (!Helper.CheckURL(url))
                        throw new ArgumentException("Not an URL");
                        List<BooruAPIPost> api_posts = BooruAPI.SearchPostsPerURL(url);
                        if (api_posts == null)
                            _Booru.AddPost(new BooruAPIPost()
                            {
                                Tags = BooruTagList.FromString(tags),
                                Private = privat,
                                ImageURL = url
                            });
                        else if (api_posts.Count < 1)
                            throw new Exception("Can't find a post");
                        else if (api_posts.Count > 1)
                            throw new Exception("Too much posts found (" + api_posts.Count + ")");
                        else
                        {
                            BooruAPIPost apiPost = api_posts[0];
                            apiPost.Tags = BooruTagList.FromString(tags);
                            apiPost.Private = privat;
                            apiPost.DownloadImage();
                            _Booru.AddPost(apiPost);
                        }
                })));
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