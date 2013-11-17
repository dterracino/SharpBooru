using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using TA.SharpBooru.BooruAPIs;

namespace TA.SharpBooru.Client.CLI
{
    public class BooruConsole : ConsoleEx
    {
        private ClientBooru _Booru;

        public BooruConsole(ClientBooru Booru)
            : base()
        { _Booru = Booru; }

        public override string Prompt { get { return "Booru> "; } }

        protected override void PopulateCommandList(List<Command> Commands, TextWriter Out)
        {
            Commands.Add(new Command("post info", "post info <ID>", new Action<ulong>(id =>
                {
                    BooruPost post = _Booru.GetPost(id, false);
                    Out.WriteLine("Post #{0} uploaded {1} by {2}:", post.ID, post.CreationDate, post.User);
                    Out.WriteLine("Viewed {0} times / Edited {1} times", post.ViewCount, post.EditCount);
                    Out.WriteLine("Rating {0}, Image size {1} x {2}", post.Rating, post.Width, post.Height);
                    Out.WriteLine("Source: {0}", post.Source);
                    Out.WriteLine("Post is {0}, Description: {1}", post.Private ? "private" : "public", post.Description);
                    Out.Write("Tags:"); post.Tags.ForEach(x => Console.Write(" {0}", x.Tag));
                    Out.WriteLine();
                })));
            Commands.Add(new Command("image save", "image save <ID> <Path>", new Action<ulong, string>((id, path) => _Booru.GetImage(id).Save(path))));
            Commands.Add(new Command("image view", "image view <ID>", new Action<ulong>(id =>
                {
                    string tempFile = Helper.GetTempFile();
                    _Booru.GetImage(id).Save(ref tempFile, true);
                    bool useFBI = string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DISPLAY")) && Helper.IsUnix();
                    if (useFBI)
                        using (Process fbiImgViewer = Process.Start("fbi", tempFile))
                            fbiImgViewer.WaitForExit();
                    else using (Process imgViewer = Process.Start(tempFile))
                            if (imgViewer != null)
                                imgViewer.WaitForExit();
                })));
            /* TODO Command image dupes
            Commands.Add(new Command("image dupes", "image dupes <Folder>", new Action<string>(folder =>
                {
                    string[] files = Directory.GetFiles(folder);
                    foreach (string file in files)
                        using (BooruImage img = BooruImage.FromFile(file))
                        {
                            Console.Write("{0} -", Path.GetFileName(file));
                            ulong hash = img.CalculateImageHash();
                            List<ulong> dupes = _Booru.FindImageDupes(hash);
                            if (dupes.Count > 0)
                                foreach (ulong dupeID in dupes)
                                    Console.WriteLine(" {0}", dupeID);
                            else Console.WriteLine(" no dupes");
                        }
                })));
            */
            Commands.Add(new Command("tag delete", "tag delete <ID>", new Action<ulong>(id => _Booru.DeleteTag(id))));
            Commands.Add(new Command("user change", "user change <Username> <Password>", new Action<string, string>((un, pw) => _Booru.ChangeUser(un, pw))));
            Commands.Add(new Command("user add", "user add <Username> <Password> <CanAddPosts> <CanDeletePosts> <CanEditPosts> <CanDeleteTags> <CanEditTags> <CanLoginDirect> <CanLoginOnline> <AdvancePostControl> <IsAdmin> <MaxRating>", new Action<string, string, bool, bool, bool, bool, bool, bool, bool, bool, bool, ushort>((un, pw, cap, cdp, cep, cdt, cet, cld, clo, apc, ia, mr) =>
                {
                    BooruUser user = new BooruUser()
                    {
                        Username = un,
                        Password = pw,
                        CanAddPosts = cap,
                        CanDeletePosts = cdp,
                        CanEditPosts = cep,
                        CanDeleteTags = cdt,
                        CanEditTags = cet,
                        CanLoginDirect = cld,
                        CanLoginOnline = clo,
                        AdvancePostControl = apc,
                        IsAdmin = ia,
                        MaxRating = mr
                    };
                    _Booru.AddUser(user);
                })));
            Commands.Add(new Command("user delete", "user delete <Username>", new Action<string>(un => _Booru.DeleteUser(un))));
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
            Commands.Add(new Command("post add", "post add <ImgPath> <Tags> <Private>", new Action<string, string, bool>((imgpath, tags, privat) =>
                {
                    if (!File.Exists(imgpath))
                        throw new ArgumentException("Image not found");
                    using (BooruImage img = BooruImage.FromFile(imgpath))
                    {
                        BooruPost post = new BooruPost()
                        {
                            Description = "Imported via CLI",
                            CreationDate = DateTime.Now,
                            Image = img,
                            ImageHash = img.CalculateImageHash(),
                            Private = privat,
                            Tags = BooruTagList.FromString(tags)
                        };
                        _Booru.AddPost(post);
                    }
                })));
            //TODO Command post edit
            Commands.Add(new Command("tag getid", "tag getid <Tag>", new Action<string>(tag => Out.WriteLine(_Booru.GetTag(tag).ID))));
            Commands.Add(new Command("tag chtype", "tag chtype <TagID> <TypeString>", new Action<ulong, string>((tagid, type) =>
                {
                    BooruTag tag = _Booru.GetTag(tagid);
                    tag.Type = type;
                    _Booru.SaveTag(tag);
                })));
            Commands.Add(new Command("alias add", "alias add <Alias> <TagID>", new Action<string, ulong>((alias, tagid) => _Booru.AddAlias(alias, tagid))));
        }
    }
}