using System;

namespace TA.SharpBooru.Server
{
    public static class SQLStatements
    {
        //Selects
        public const string GetTags = "SELECT * FROM tags";
        public const string GetPostByID = "SELECT * FROM posts WHERE id = ?";
        public const string GetUserByUsername = "SELECT * FROM users WHERE username = ?";
        public const string GetTagsByPostID = "SELECT tags.* FROM tags INNER JOIN post_tags ON post_tags.tag = tags.id WHERE post_tags.post = ?";
        public const string GetTagByTagString = "SELECT * FROM tags WHERE tag = ?";

        //Counts
        public const string GetPostCountByID = "SELECT COUNT(*) FROM posts WHERE id = ?";
        public const string GetTagCountByID = "SELECT COUNT(*) FROM tags WHERE id = ?";
        public const string GetTagCount = "SELECT COUNT(*) FROM tags";
        public const string GetUserCount = "SELECT COUNT(*) FROM users";
        public const string GetPostCount = "SELECT COUNT(*) FROM posts";

        //Deletions
        public const string DeletePostByID = "DELETE FROM posts WHERE id = ?";
        public const string DeleteTagByID = "DELETE FROM tags WHERE id = ?1";
        public const string DeletePostTagsByPostID = "DELETE FROM post_tags WHERE post = ?";
        public const string DeletePostTagsByTagID = "DELETE FROM post_tags WHERE tag = ?";

        //Insertions
        //LOL NO
    }
}
