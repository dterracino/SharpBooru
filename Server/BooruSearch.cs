using System;
using System.Data;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TA.SharpBooru.Server
{
    public static class BooruSearch
    {
        //TODO Implement allowedVariables
        //private static string[] allowedVariables = new string[] { "ViewCount", "EditCount", "Score", "Rating", "ID", "Width", "Height", "Private" };
        private static Regex testSpecialRegex = new Regex(".+(<|>|=|!|<=|>=|<>).+");
        private static Regex operatorRegex = new Regex("<|>|=|!|<=|>=|<>");

        private class SpecialPattern
        {
            public string Variable;
            public string Value;
            public int Operator;
            public bool Negate = false;

            public bool CheckPost(BooruPost Post) { return nonNegatedCheckPost(Post) ^ Negate; }

            private bool nonNegatedCheckPost(BooruPost Post)
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

        public static BooruPostList DoSearch(string Pattern, ServerBooru Booru)
        {
            string[] parts = SplitString(Pattern);
            if (parts.Length < 1)
                using (DataTable postTable = Booru.DB.ExecuteTable(SQLStatements.GetPosts))
                    return BooruPostList.FromTable(postTable);

            //Get all posts
            //Perform all the special patterns
            //return the post ids

            List<string> tagSearchQueries = new List<string>();
            var specialPatterns = new List<SpecialPattern>();

            //Extract all the needed information
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                bool negate = ExtractNegate(ref part);

                if (!IsSpecialPattern(part))
                {
                    DataRow tagRow = Booru.DB.ExecuteRow(SQLStatements.GetTagByTagString, part);
                    BooruTag tag = BooruTag.FromRow(tagRow);
                    if (tag != null)
                        tagSearchQueries.Add(string.Format("id {0} (SELECT post FROM post_tags WHERE tag = {1})", negate ? "NOT IN" : "IN", tag.ID));
                }
                else
                {
                    SpecialPattern sPattern = ExtractSpecialPattern(part);
                    sPattern.Negate = negate;
                    specialPatterns.Add(sPattern);
                }
            }

            string tagSearchQuery = tagSearchQueries.Count > 0 ?
                "SELECT * FROM posts WHERE " + string.Join(" AND ", tagSearchQueries) + " ORDER BY creationdate DESC"
                : SQLStatements.GetPosts;
            using (DataTable postTable = Booru.DB.ExecuteTable(tagSearchQuery))
            {
                BooruPostList postList = new BooruPostList();
                foreach (BooruPost post in BooruPostList.FromTable(postTable))
                    if (DoSpecialPatternChecks(specialPatterns, post))
                        postList.Add(post);
                return postList;
            }
        }

        private static bool DoSpecialPatternChecks(List<SpecialPattern> Patterns, BooruPost Post)
        {
            foreach (var check in Patterns)
                if (!(check.CheckPost(Post)))
                    return false;
            return true;
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

        private static bool IsSpecialPattern(string Pattern) { return testSpecialRegex.IsMatch(Pattern); }

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
