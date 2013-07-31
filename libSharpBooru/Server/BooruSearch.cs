using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TA.SharpBooru.Server
{
    public static class BooruSearch
    {
        private static string[] allowedVariables = new string[] { "ViewCount", "EditCount", "Score", "Rating", "ID", "Width", "Height", "Private" };
        private static Regex operatorRegex = new Regex("<|>|=|!|<=|>=|<>");

        private class SpecialPattern
        {
            public string Variable;
            public string Value;
            public int Operator;

            public bool CheckPost(BooruPost Post)
            {
                FieldInfo[] fields = typeof(BooruPost).GetFields();
                try
                {
                    foreach (FieldInfo field in fields)
                        if (field.Name.ToLower() == Variable)
                        {
                            object convertedValue = Convert.ChangeType(Value, field.FieldType);
                            object fieldValue = field.GetValue(Post);
                            switch (Operator)
                            {
                                case 0: return Convert.ToInt64(fieldValue) < Convert.ToInt64(convertedValue);
                                case 1: return Convert.ToInt64(fieldValue) <= Convert.ToInt64(convertedValue);
                                case 3: return Convert.ToInt64(fieldValue) >= Convert.ToInt64(convertedValue);
                                case 4: return Convert.ToInt64(fieldValue) > Convert.ToInt64(convertedValue);
                                case 2: return convertedValue.Equals(fieldValue);
                                case 5: return !convertedValue.Equals(fieldValue);
                            }
                        }
                }
                catch { }
                return false;
            }
        }

        public static BooruPostList DoSearch(string Pattern, BooruPostList Posts, BooruTagList Tags)
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
                    try
                    {
                        SpecialPattern sPattern = ExtractSpecialPattern(part);
                        foreach (BooruPost post in Posts)
                            if (sPattern.CheckPost(post))
                                newPosts.Add(post);
                    }
                    catch { }
                else
                {
                    BooruTag tag = Tags[part];
                    if (tag != null)
                    {
                        foreach (BooruPost post in Posts)
                            if (negate ^ post.TagIDs.Contains(tag.ID))
                                newPosts.Add(post);
                    }
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

        private static bool IsSpecialPattern(string Pattern) { return operatorRegex.IsMatch(Pattern); }

        private static SpecialPattern ExtractSpecialPattern(string Pattern)
        {
            Match operatorMatch = operatorRegex.Match(Pattern);
            string _variable = Pattern.Substring(0, operatorMatch.Index);
            string _operator = operatorMatch.Value;
            Pattern = Pattern.Substring(_variable.Length + _operator.Length);
            if (operatorRegex.IsMatch(Pattern))
                Pattern = Pattern.Substring(1);
            string _value = Pattern;
            return new SpecialPattern()
            {
                Operator = ExtractOperator(_operator),
                Variable = _variable,
                Value = _value
            };
        }

        private static int ExtractOperator(string TypeString)
        {
            switch (TypeString.Trim())
            {
                default: return -1;
                case "<": return 0;
                case "<=": return 1;
                case ">": return 4;
                case ">=": return 3;
                case "!":
                case "!=":
                case "<>": return 5;
                case "=":
                case "==": return 2;
            }
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
