﻿using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LitJson;

namespace TA.SharpBooru.BooruAPIs
{
    public abstract class BooruAPI
    {
        public abstract List<BooruAPIPost> SearchPosts(string[] Pattern);
        //public abstract List<BooruAPITag> SearchTags(string[] Pattern);
        protected abstract string GetBaseURI();

        public virtual BooruAPIPost GetSinglePost(string ID)
        {
            List<BooruAPIPost> posts = SearchPosts(new string[1] { string.Format("id:{0}", Convert.ToInt32(ID)) });
            if (posts.Count != 1)
                return null;
            else return posts[0];
        }

        protected string CreateURI(params string[] Params)
        {
            if (Params.Length % 2 > 0)
                throw new ArgumentException("Length % 2 must be 0");
            List<KeyValuePair<string, string>> dict = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < Params.Length; i += 2)
                if (!string.IsNullOrWhiteSpace(Params[i]))
                    if (!string.IsNullOrWhiteSpace(Params[i + 1]))
                        dict.Add(new KeyValuePair<string, string>(
                            Params[i].Trim().ToLower(),
                            Params[i + 1].Trim().ToLower()
                        ));
            return CreateURI(dict.ToArray());
        }

        protected string CreateURI(KeyValuePair<string, string>[] Params)
        {
            List<string> paramStrings = new List<string>();
            foreach (KeyValuePair<string, string> pair in Params)
            {
                if (!string.IsNullOrWhiteSpace(pair.Key))
                    if (!string.IsNullOrWhiteSpace(pair.Value))
                        paramStrings.Add(string.Format(
                            "{0}={1}",
                            pair.Key.Trim().ToLower(),
                            pair.Value.Trim().ToLower()
                        ));
            }
            if (paramStrings.Count < 1)
                return GetBaseURI();
            else
            {
                string baseuri = GetBaseURI();
                char delimiter = baseuri.Contains('?') ? '&' : '?';
                return GetBaseURI() + delimiter + string.Join("&", paramStrings);
            }
        }

        protected XmlDocument GetXmlDocument(string URI)
        {
            XmlDocument document = new XmlDocument();
            using (XmlReader reader = XmlReader.Create(URI))
                document.Load(reader);
            return document;
        }

        protected JsonData GetJSONData(string URI)
        {
            string temp = Helper.DownloadTemporary(URI);
            if (temp != null)
                using (StreamReader sr = new StreamReader(temp))
                {
                    JsonReader reader = new JsonReader(sr);
                    JsonData data = JsonMapper.ToObject(reader);
                    reader.Close();
                    return data;
                }
            else return null;
        }

        protected void CheckForTags(ref List<BooruAPIPost> Posts, string[] Tags)
        {
            for (int i = Posts.Count - 1; !(i < 0); i--)
                foreach (string tag in Tags)
                {
                    string ntag = tag.Trim().ToLower();
                    bool negate = ntag.StartsWith("-");
                    if (negate)
                        ntag = ntag.Substring(1);
                    if (!(Posts[i].Tags.Contains(ntag) ^ negate))
                    {
                        Posts.RemoveAt(i);
                        break; 
                    }
                }
        }

        protected static BooruTagList GetTagList(string Pattern)
        {
            if (string.IsNullOrWhiteSpace(Pattern))
                return new BooruTagList();
            else
            {
                BooruTagList tagList = new BooruTagList();
                Pattern = Pattern.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
                foreach (string str_tag in Pattern.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    tagList.Add(new BooruTag(str_tag));
                return tagList;
            }
        }

        public static List<BooruAPIPost> SearchPostsPerURL(string URL)
        {
            object result = internalSearchPostsPerURL(URL);
            if (result is List<BooruAPIPost>)
                return (List<BooruAPIPost>)result;
            else if (result is BooruAPIPost)
            {
                List<BooruAPIPost> list = new List<BooruAPIPost>();
                list.Add((BooruAPIPost)result);
                return list;
            }
            else return null;
        }

        private static string ExtractParameterFromURLQuery(string URL, string Param)
        {
            int ss_begin = URL.IndexOf(Param + "=");
            if (!(ss_begin < 0))
                ss_begin += Param.Length + 1;
            else return string.Empty;
            int ss_end = URL.IndexOf('&', ss_begin);
            if (ss_end < 0)
                ss_end = URL.Length;
            return URL.Substring(ss_begin, ss_end - ss_begin);
        }

        private static object internalSearchPostsPerURL(string URL)
        {
            if (Helper.CheckURL(URL))
            {
                //TODO Add pool detection
                //TODO Gelbooru/Safebooru failing when &pool_id= is appended to URL
                URL = URL.Trim().ToLower();
                if (Regex.IsMatch(URL, "http://(www.|)gelbooru.com/index.php\\?page=post&s=view&id=[0-9]*"))
                    return (new GelbooruAPI()).GetSinglePost(URL.Substring(URL.LastIndexOf("=") + 1));
                else if (Regex.IsMatch(URL, "http://(www.|)gelbooru.com/index.php\\?page=post&s=list"))
                    return (new GelbooruAPI()).SearchPosts(ExtractParameterFromURLQuery(URL, "tags").Split('+'));
                else if (Regex.IsMatch(URL, "http://(www.|)konachan.com/post/show/[0-9]*/?.*"))
                    return (new KonachanAPI(true)).GetSinglePost(Regex.Match(URL, "show/[0-9]{1,}").Value.Substring(5));
                else if (Regex.IsMatch(URL, "http://(www.|)konachan.com/post.*"))
                    return (new KonachanAPI(true)).SearchPosts(ExtractParameterFromURLQuery(URL, "tags").Split('+'));
                else if (Regex.IsMatch(URL, "http://(www.|)konachan.net/post/show/[0-9]*/?.*"))
                    return (new KonachanAPI(false)).GetSinglePost(Regex.Match(URL, "show/[0-9]{1,}").Value.Substring(5));
                else if (Regex.IsMatch(URL, "http://(www.|)konachan.net/post.*"))
                    return (new KonachanAPI(false)).SearchPosts(ExtractParameterFromURLQuery(URL, "tags").Split('+'));
                if (Regex.IsMatch(URL, "http://(www.|)safebooru.org/index.php\\?page=post&s=view&id=[0-9]*"))
                    return (new SafebooruAPI()).GetSinglePost(URL.Substring(URL.LastIndexOf("=") + 1));
                else if (Regex.IsMatch(URL, "http://(www.|)safebooru.org/index.php\\?page=post&s=list"))
                    return (new SafebooruAPI()).SearchPosts(ExtractParameterFromURLQuery(URL, "tags").Split('+'));
            }
            return null;
        }

        protected BooruAPIPost CreateAPIPost(string APIName)
        {
            return new BooruAPIPost()
            {
                APIName = APIName,
                Comment = "Imported from " + APIName
            };
        }
    }
}