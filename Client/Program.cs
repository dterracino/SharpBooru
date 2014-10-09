using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Mono.Unix;

namespace TA.SharpBooru
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            string invokedVerb = null;
            object invokedVerbOptions = null;
            var mainOptions = new Options();

            if (CommandLine.Parser.Default.ParseArguments(args, mainOptions, (verb, subOptions) =>
                {
                    invokedVerb = verb;
                    invokedVerbOptions = subOptions;
                }))
            {
                try
                {
                    CommonOptions commonOptions = (CommonOptions)invokedVerbOptions;
                    UnixEndPoint endPoint = new UnixEndPoint(commonOptions.Socket);

                    using (Socket socket = new Socket(AddressFamily.Unix, SocketType.Stream, 0))
                    {
                        socket.Connect(endPoint);

                        switch (invokedVerb)
                        {
                            case "add":
                                {
                                    AddOptions options = (AddOptions)invokedVerbOptions;
                                    using (var post = new BooruPost())
                                    using (var image = BooruImage.FromFile(options.ImagePath))
                                    {
                                        foreach (var tag in options.Tags.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                                            post.Tags.Add(new BooruTag(tag));
                                        post.Source = options.Source;
                                        post.Description = options.Description;
                                        post.Rating = options.Rating;
                                        post.Private = options.Private;

                                        using (ReaderWriter rwo = new ReaderWriter(new NetworkStream(socket)))
                                        {
                                            rwo.DisposeStreams = true;
                                            if (!string.IsNullOrWhiteSpace(options.Username))
                                                using (MemoryStream ms = new MemoryStream())
                                                {
                                                    using (ReaderWriter rwi = new ReaderWriter(ms))
                                                    {
                                                        rwi.Write(options.Username, true);
                                                        rwi.Write(options.Password ?? string.Empty, true);
                                                    }
                                                    Request(rwo, RequestCode.Login, ms.ToArray());
                                                }
                                            using (MemoryStream ms = new MemoryStream())
                                            {
                                                using (ReaderWriter rwi = new ReaderWriter(ms))
                                                {
                                                    post.ToWriter(rwi);
                                                    post.Tags.ToWriter(rwi);
                                                    image.ToWriter(rwi);
                                                }
                                                using (var result_ms = new MemoryStream(Request(rwo, RequestCode.Add_Post, ms.ToArray())))
                                                using (ReaderWriter result_rw = new ReaderWriter(result_ms))
                                                    Console.WriteLine("OK - PostID = " + result_rw.ReadULong());
                                            }
                                        }
                                    }
                                }
                                return 0;
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }
            return 1;
        }

        private static byte[] Request(ReaderWriter RW, RequestCode RQ, byte[] Payload)
        {
            RW.Write((ushort)RQ);
            RW.Write(Payload, true);
            RW.Flush();
            if (RW.ReadBool())
                return RW.ReadBytes();
            else throw new Exception(RW.ReadString());
        }
    }
}
