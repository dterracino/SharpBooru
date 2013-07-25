using System;

namespace TA.SharpBooru.Server
{
    public static class BooruSearch
    {
        public static BooruPostList DoSearch(string Pattern, BooruPostList Posts)
        {
            string[] parts = SplitString(Pattern);
            if (parts.Length < 1)
                return Posts;

            for (int i = 0; i < parts.Length; i++)
            {
                BooruPostList newPosts = new BooruPostList();
                string part = parts[i];
                bool negate = ExtractNegate(ref part);

                if (IsSpecialPattern(part))
                {
                    //TODO Implement special patterns
                }
                else foreach (BooruPost post in Posts)
                    {
                        bool containsTag = post.Tags.Contains(part);
                        if (containsTag ^ negate)
                            newPosts.Add(post);
                    }

                Posts = newPosts;
            }

            return Posts;
        }

        private static bool ExtractNegate(ref string Pattern)
        {
            if (Pattern.StartsWith("-"))
            {
                Pattern = Pattern.Substring(1);
                return true;
            }
            else return false;
        }

        private static bool IsSpecialPattern(string Pattern)
        {
            return false; //TODO Implement special pattern RegEx
        }

        private static string[] SplitString(string String)
        {
            if (!string.IsNullOrWhiteSpace(String))
            {
                String = String.ToLower();
                return String.Split(
                    new char[4] { '\r', '\n', '\t', ' ' },
                    StringSplitOptions.RemoveEmptyEntries);
            }
            else return new string[0];
        }
    }
}
