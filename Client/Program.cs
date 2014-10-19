using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections.Generic;
using TA.SharpBooru.BooruAPIs;
using CommandLine;
using Mono.Unix;

namespace TA.SharpBooru
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var pResult = Parser.Default.ParseArguments(args, new Type[]
            {
                typeof(AddOptions),
                typeof(AddUrlOptions),
                typeof(DelOptions),
                typeof(GetOptions),
                typeof(EditOptions),
                typeof(EditImgOptions)
            });

            if (!pResult.Errors.Any())
            {
                try
                {
                    var commonOptions = (Options)pResult.Value;
                    Config config = Config.TryLoad();
                    if (config != null)
                    {
                        if (commonOptions.Socket == null)
                            commonOptions.Socket = config.Socket;
                        if (commonOptions.Username == null)
                            commonOptions.Username = config.Username;
                        if (commonOptions.Password == null)
                            commonOptions.Password = config.Password;
                    }
                    else if (commonOptions.Socket == null && File.Exists("socket.sock"))
                        commonOptions.Socket = "socket.sock";
                    else if (commonOptions.Socket == null && File.Exists("/srv/booru/socket.sock"))
                        commonOptions.Socket = "/etc/booru/socket.sock";

                    UnixEndPoint endPoint = new UnixEndPoint(commonOptions.Socket);

                    using (Socket socket = new Socket(AddressFamily.Unix, SocketType.Stream, 0))
                    {
                        socket.Connect(endPoint);
                        using (NetworkStream ns = new NetworkStream(socket, true))
                        {
                            if (!string.IsNullOrWhiteSpace(commonOptions.Username))
                                Request(ns, RequestCode.Login, (rw) =>
                                    {
                                        rw.Write(commonOptions.Username, true);
                                        rw.Write(commonOptions.Password ?? string.Empty, true);
                                    }, (rw) => { });

                            Type oType = commonOptions.GetType();
                            if (oType == typeof(AddOptions))
                            {
                                var options = (AddOptions)commonOptions;
                                Console.Write("Loading image... ");
                                using (var post = new BooruPost())
                                using (var image = BooruImage.FromFile(options.ImagePath))
                                {
                                    Console.WriteLine("OK");
                                    foreach (var tag in options.Tags.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                                        post.Tags.Add(new BooruTag(tag));
                                    post.Source = options.Source;
                                    post.Description = options.Description;
                                    post.Rating = (byte)options.Rating;
                                    post.Private = options.Private;
                                    Console.Write("Adding post... ");
                                    ulong id = AddPost(ns, post, post.Tags, image);
                                    Console.WriteLine(id);
                                }
                            }
                            else if (oType == typeof(AddUrlOptions)) //TODO Only import one image
                            {
                                var options = (AddUrlOptions)commonOptions;
                                var apiPosts = BooruAPI.SearchPostsPerURL(options.URL);
                                if (apiPosts.Count < 1)
                                    throw new Exception("No post to import detected");
                                else if (apiPosts.Count > 1)
                                {
                                    Console.WriteLine("Multiple posts found, importing only the first one");
                                    for (int i = 1; i < apiPosts.Count; i++)
                                        apiPosts[i].Dispose();
                                }
                                var apiPost = apiPosts[0];
                                if (options.CustomImagePath == null)
                                {
                                    Console.Write("Downloading image... ");
                                    apiPost.DownloadImage();
                                }
                                else
                                {
                                    Console.Write("Loading image... ");
                                    apiPost.Image = BooruImage.FromFile(options.CustomImagePath);
                                }
                                Console.WriteLine("OK");
                                if (!options.AllTags)
                                {
                                    string[] allTags = null;
                                    Request(ns, RequestCode.Get_AllTags, (rw) => { }, (rw) =>
                                        {
                                            uint count = rw.ReadUInt();
                                            allTags = new string[count];
                                            for (int a = 0; a < count; a++)
                                                allTags[a] = rw.ReadString();
                                        });
                                    for (int a = apiPost.Tags.Count - 1; !(a < 0); a--)
                                        if (!allTags.Contains(apiPost.Tags[a].Tag))
                                            apiPost.Tags.RemoveAt(a);
                                }
                                if (options.Tags != null)
                                {
                                    options.Tags = options.Tags.ToLower();
                                    if (options.TagsNoDelta)
                                    {
                                        string[] parts = options.Tags.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                        apiPost.Tags.Clear();
                                        foreach (string part in parts)
                                            apiPost.Tags.Add(new BooruTag(part));
                                    }
                                    else TagDelta(ref apiPost.Tags, options.Tags);
                                }
                                if (options.Description == null)
                                    apiPost.Description = "Imported from " + apiPost.APIName;
                                else apiPost.Description = options.Description;
                                apiPost.Rating = (byte)options.Rating;
                                apiPost.Private = options.Private;
                                Console.Write("Importing post... ");
                                ulong id = AddPost(ns, apiPost, apiPost.Tags, apiPost.Image);
                                Console.WriteLine(id);
                            }
                            else if (oType == typeof(DelOptions))
                            {
                                var options = (DelOptions)commonOptions;
                                Request(ns, RequestCode.Delete_Post, (rw) => rw.Write(options.ID), (rw) => { });
                            }
                            else if (oType == typeof(GetOptions))
                            {
                                var options = (GetOptions)commonOptions;
                                using (var post = GetPost(ns, options.ID))
                                {
                                    Console.WriteLine("User        " + post.User);
                                    Console.WriteLine("Private     " + (post.Private ? "yes" : "no"));
                                    Console.WriteLine("Source      " + post.Source ?? string.Empty);
                                    Console.WriteLine("Description " + post.Description ?? string.Empty);
                                    Console.WriteLine("Rating      " + post.Rating);
                                    Console.WriteLine("Size        {0}x{1}", post.Width, post.Height);
                                    Console.WriteLine("Date        {0}", post.CreationDate);
                                    Console.WriteLine("ViewCount   " + post.ViewCount);
                                    Console.WriteLine("EditCount   " + post.EditCount);
                                    Console.WriteLine("Score       " + post.Score);
                                    Console.WriteLine();
                                    Console.WriteLine(BooruTagListToString(post.Tags));
                                }
                            }
                            else if (oType == typeof(EditOptions))
                            {
                                var options = (EditOptions)commonOptions;
                                using (var post = GetPost(ns, options.ID))
                                {
                                    post.EditCount += 1;
                                    if (options.Description != null)
                                        post.Description = options.Description;
                                    if (options.Private.HasValue)
                                        post.Private = options.Private.Value;
                                    if (!(options.Rating < 0) && options.Rating < byte.MaxValue)
                                        post.Rating = (byte)options.Rating;
                                    if (options.Source != null)
                                        post.Source = options.Source;
                                    if (options.Tags != null)
                                    {
                                        options.Tags = options.Tags.ToLower();
                                        if (options.TagsNoDelta)
                                        {
                                            string[] parts = options.Tags.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                            post.Tags.Clear();
                                            foreach (string part in parts)
                                                post.Tags.Add(new BooruTag(part));
                                        }
                                        else TagDelta(ref post.Tags, options.Tags);
                                    }
                                    Request(ns, RequestCode.Edit_Post, (rw) =>
                                        {
                                            post.ToWriter(rw);
                                            post.Tags.ToWriter(rw);
                                        }, (rw) => { });
                                }
                            }
                            else if (oType == typeof(EditImgOptions))
                            {
                                EditImgOptions options = (EditImgOptions)commonOptions;
                                BooruImage img = null;
                                string path = options.Path;
                                try
                                {
                                    Request(ns, RequestCode.Get_Image, (rw) => rw.Write(options.ID), (rw) => { img = BooruImage.FromReader(rw); });
                                    img.Save(ref path, true);
                                }
                                finally { img.Dispose(); }
                                if (options.Tool != null)
                                {
                                    var psi = new ProcessStartInfo(options.Tool, path);
                                    Process tool = new Process() { StartInfo = psi };
                                    tool.Start();
                                    Console.Write("Waiting for image editor to exit...");
                                    tool.WaitForExit();
                                }
                                else
                                {
                                    Console.Write("Edit the image and press any key to save it... ");
                                    Console.ReadKey(true);
                                    Console.WriteLine();
                                }
                                using (BooruImage eImg = BooruImage.FromFile(path))
                                    Request(ns, RequestCode.Edit_Image, (rw) =>
                                        {
                                            rw.Write(options.ID);
                                            eImg.ToWriter(rw);
                                        }, (rw) => { });
                                File.Delete(options.Path);
                            }
                            //Logout
                            ns.WriteByte(0xFF);
                            ns.WriteByte(0xFF);
                        }
                        return 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.GetType().FullName + ": " + ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
            return 1;
        }

        private static void TagDelta(ref BooruTagList Tags, string deltaString)
        {
            var removeTags = new List<string>();
            var addTags = new List<string>();
            string[] deltaParts = deltaString.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in deltaParts)
                if (part.StartsWith("_") && part.Length > 1)
                    removeTags.Add(part.Substring(1).ToLower());
                else addTags.Add(part.ToLower());
            for (int i = Tags.Count - 1; !(i < 0); i--)
                if (removeTags.Contains(Tags[i].Tag))
                    Tags.RemoveAt(i);
            foreach (string addTag in addTags)
                Tags.Add(new BooruTag(addTag));
        }

        private static string BooruTagListToString(BooruTagList Tags)
        {
            string[] strTags = new string[Tags.Count];
            for (int i = 0; i < strTags.Length; i++)
            {
                string tag = Tags[i].Tag;
                var color = Tags[i].Color;
                strTags[i] = string.Format("\x1b[38;2;{0};{1};{2}m{3}", color.R, color.G, color.B, tag);
            }
            return string.Join(" ", strTags) + "\x1b[0m";
        }

        private static BooruPost GetPost(NetworkStream NS, ulong ID)
        {
            BooruPost post = null;
            Request(NS, RequestCode.Get_Post, (rw) => rw.Write(ID), (rw) => { post = BooruPost.FromReader(rw); });
            Request(NS, RequestCode.Get_PostTags, (rw) => rw.Write(ID), (rw) => { post.Tags = BooruTagList.FromReader(rw); });
            return post;
        }

        private static ulong AddPost(NetworkStream NS, BooruPost Post, BooruTagList Tags, BooruImage Image)
        {
            ulong postID = 0;
            Request(NS, RequestCode.Add_Post, (rw) =>
            {
                Post.ToWriter(rw);
                Tags.ToWriter(rw);
                Image.ToWriter(rw);
            }, (rw) => { postID = rw.ReadULong(); });
            return postID;
        }

        private static void Request(NetworkStream NS, RequestCode RQ, Action<ReaderWriter> ReqCB, Action<ReaderWriter> RespCB)
        {
            using (ReaderWriter rw = new ReaderWriter(NS))
            {
                rw.Write((ushort)RQ);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (ReaderWriter req_rw = new ReaderWriter(ms))
                        ReqCB(req_rw);
                    rw.Write(ms.ToArray(), true);
                }
                rw.Flush();
                if (rw.ReadBool())
                    using (MemoryStream ms = new MemoryStream(rw.ReadBytes()))
                    using (ReaderWriter resp_rw = new ReaderWriter(ms))
                        RespCB(resp_rw);
                else throw new Exception(rw.ReadString());
            }
        }
    }
}
