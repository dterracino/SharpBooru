using System;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

namespace TA.SharpBooru
{
    public class Server : IDisposable
    {
        private List<Thread> _RunningThreads = new List<Thread>();
        private object _ListLock = new object();
        private bool _IsRunning = true;
        private ServerBooru _Booru;
        private Logger _Logger;

        public Server(ServerBooru Booru, Logger Logger)
        {
            _Booru = Booru;
            _Logger = Logger;
        }

        public void AddAcceptedSocket(Socket Socket)
        {
            Thread thread = new Thread(new ThreadStart(() => _ClientHandlerStage1(Socket)));
            thread.Name = "ClientHandler";
            thread.Start();
            lock (_ListLock)
                _RunningThreads.Add(thread);
        }

        public void Dispose()
        {
            _IsRunning = false;
            lock (_ListLock)
                foreach (Thread thread in _RunningThreads)
                    thread.Abort();
        }

        private void _ClientHandlerStage1(Socket Socket)
        {
            try
            {
                using (var netStream = new NetworkStream(Socket))
                    _ClientHandlerStage2(netStream);
            }
            catch (Exception ex)
            {
                if (_IsRunning)
                    _Logger.LogException("ClientHandlerStage1", ex);
            }
        }

        private void _ClientHandlerStage2(Stream Stream)
        {
            ReaderWriter rw = new ReaderWriter(Stream);
            BooruUser user = _Booru.Login(null, "guest", "guest");
            while (_IsRunning)
            {
                ushort requestCode = rw.ReadUShort();
                byte[] payload = rw.ReadBytes();
                using (var inputMs = new MemoryStream(payload))
                using (var outputMs = new MemoryStream())
                {
                    try
                    {
                        using (var rw2 = new ReaderWriter(inputMs, outputMs))
                            _ClientHandlerStage3((RequestCode)requestCode, rw2, ref user);
                        rw.Write(true);
                        var outputBytes = outputMs.ToArray();
                        if (outputBytes.Length > 0)
                            rw.Write(outputBytes, true);
                    }
                    catch { rw.Write(false); }
                }
            }
        }

        private void _ClientHandlerStage3(RequestCode RQ, ReaderWriter RW, ref BooruUser User)
        {
            switch (RQ)
            {
                case RequestCode.Get_Post:
                    {
                        ulong id = RW.ReadULong();
                        using (var post = _Booru.GetPost(User, id, false))
                            post.ToWriter(RW);
                    } break;

                case RequestCode.Get_Thumb:
                    {
                        ulong id = RW.ReadULong();
                        using (var thumb = _Booru.GetThumbnail(User, id))
                            thumb.ToWriter(RW);
                    } break;

                case RequestCode.Get_Image:
                    {
                        ulong id = RW.ReadULong();
                        using (var image = _Booru.GetImage(User, id))
                            image.ToWriter(RW);
                    } break;

                case RequestCode.Get_Tag:
                    {
                        ulong id = RW.ReadULong();
                        _Booru.GetTag(User, id).ToWriter(RW);
                    } break;

                case RequestCode.Get_Info:
                    _Booru.BooruInfo.ToWriter(RW);
                    break;

                case RequestCode.Get_AllTags:
                    {
                        List<string> tags = _Booru.GetAllTags();
                        RW.Write((uint)tags.Count);
                        foreach (string tag in tags)
                            RW.Write(tag, true);
                    } break;

                case RequestCode.Get_PostTags:
                    {
                        ulong id = RW.ReadULong();
                        using (var post = _Booru.GetPost(User, id, false))
                            post.Tags.ToWriter(RW);
                    } break;

                case RequestCode.Get_User:
                    User.ToWriter(RW);
                    break;

                case RequestCode.Search_String: //User limitations?
                    {
                        string pattern = RW.ReadString();
                        using (var posts = BooruSearch.DoSearch(pattern, _Booru))
                        {
                            RW.Write((uint)posts.Count);
                            foreach (var post in posts)
                                RW.Write(post.ID);
                        }
                    } break;

                case RequestCode.Search_Image:
                    throw new NotImplementedException(); //TODO Implement

                case RequestCode.Login:
                    {
                        string username = RW.ReadString();
                        string password = RW.ReadString();
                        _Booru.Login(User, username, password);
                    } break;

                case RequestCode.Add_Post:
                    using (var post = BooruPost.FromReader(RW))
                    {
                        post.Tags = BooruTagList.FromReader(RW);
                        post.Image = BooruImage.FromReader(RW);
                        ulong id = _Booru.AddPost(User, post);
                        RW.Write(id);
                    } break;
            }
        }
    }
}
