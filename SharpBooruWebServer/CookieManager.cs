using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;

namespace TA.SharpBooru.Client.WebServer
{
    public class CookieManager
    {
        private class UniqueDict<T1, T2>
        {
            private List<T1> _Obj1 = new List<T1>();
            private List<T2> _Obj2 = new List<T2>();
            private object _Lock = new object();

            public void Add(T1 Object1, T2 Object2)
            {
                if (Object1 == null || Object2 == null)
                    throw new ArgumentNullException();
                lock (_Lock)
                {
                    if (_Obj1.Contains(Object1))
                        throw new ArgumentException("Object1 already exists");
                    if (_Obj2.Contains(Object2))
                        throw new ArgumentException("Object2 already exists");
                    _Obj1.Add(Object1);
                    _Obj2.Add(Object2);
                }
            }

            public bool Contains(T1 Key) { lock (_Lock) return _Obj1.Contains(Key); }
            public bool Contains(T2 Key) { lock (_Lock) return _Obj2.Contains(Key); }

            public T2 Get(T1 Key)
            {
                lock (_Lock)
                {
                    if (_Obj1.Contains(Key))
                        return _Obj2[_Obj1.IndexOf(Key)];
                    else throw new KeyNotFoundException();
                }
            }

            public T1 Get(T2 Key)
            {
                lock (_Lock)
                {
                    if (_Obj2.Contains(Key))
                        return _Obj1[_Obj2.IndexOf(Key)];
                    else throw new KeyNotFoundException();
                }
            }

            public void Remove(T1 Key)
            {
                lock (_Lock)
                    if (_Obj1.Contains(Key))
                    {
                        _Obj2.RemoveAt(_Obj1.IndexOf(Key));
                        _Obj1.Remove(Key);
                    }
            }

            public void Remove(T2 Key)
            {
                lock (_Lock)
                    if (_Obj2.Contains(Key))
                    {
                        _Obj1.RemoveAt(_Obj2.IndexOf(Key));
                        _Obj2.Remove(Key);
                    }
            }
        }

        private string _CookieName = "TA_CookieManager_SessionCookie";
        private Random _Random = new Random();
        private UniqueDict<BooruUser, Cookie> _CookieList = new UniqueDict<BooruUser, Cookie>();

        public string CookieName
        {
            get { return _CookieName; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    _CookieName = value;
                else throw new ArgumentException("Invalid name");
            }
        }

        public Cookie GetCookie(BooruUser User)
        {
            if (_CookieList.Contains(User))
                return _CookieList.Get(User);
            else
            {
                Cookie tmpCookie = new Cookie(CookieName, Helper.RandomString(_Random, 64));
                tmpCookie.Expires = DateTime.MinValue;
                _CookieList.Add(User, tmpCookie);
                return tmpCookie;
            }
        }

        public void DeleteCookie(Cookie Cookie)
        {
            if (_CookieList.Contains(Cookie))
                _CookieList.Remove(Cookie);
        }

        public void DeleteUser(BooruUser User)
        {
            if (_CookieList.Contains(User))
                _CookieList.Remove(User);
        }

        public BooruUser GetUser(CookieCollection Cookies)
        {
            if (Cookies != null)
                foreach (Cookie tmpCookie in Cookies)
                    if (_CookieList.Contains(tmpCookie))
                        return _CookieList.Get(tmpCookie);
            return null;
        }

        public BooruUser GetUser(Cookie Cookie)
        {
            if (_CookieList.Contains(Cookie))
                return _CookieList.Get(Cookie);
            else return null;
        }
    }
}