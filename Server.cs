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
                        rw.Write(outputMs.ToArray(), true);
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

                case RequestCode.Search: //User limitations?
                    {
                        string pattern = RW.ReadString();
                        BooruPostList posts = BooruSearch.DoSearch(pattern, _Booru);
                        RW.Write((uint)posts.Count);
                        foreach (var post in posts)
                            RW.Write(post.ID);
                    } break;

                case RequestCode.Search_Img:
                    throw new NotImplementedException(); //TODO Implement

                case RequestCode.Login:
                    {
                        string username = RW.ReadString();
                        string password = RW.ReadString();
                        _Booru.Login(User, username, password).ToWriter(RW);
                    } break;
            }
        }
    }
}
