namespace TA.SharpBooru.Server
{
    public static class SQLStatements
    {
        //Selects
        public const string GetPostByID = "SELECT * FROM posts WHERE id = ?";
        public const string GetUserByUsername = "SELECT * FROM users WHERE username = ?";
        public const string GetTags = @"
SELECT tags.id AS id, tags.tag AS tag, tag_types.type AS type, tag_types.description AS description,
tag_types.color AS color FROM tags INNER JOIN tag_types ON type_id = tag_types.id";
        public const string GetPosts = "SELECT * FROM posts ORDER BY creationdate DESC";
        public const string GetTagsByPostID = @"
SELECT tags.id AS id, tags.tag AS tag, tag_types.type AS type, tag_types.description AS description,
tag_types.color AS color FROM (SELECT tags.* FROM tags INNER JOIN post_tags ON tags.id = post_tags.tag
WHERE post_tags.post = ?) AS tags INNER JOIN tag_types ON type_id = tag_types.id";
        public const string GetTagsByTerm = @"
SELECT tags.id AS id, tags.tag AS tag, tag_types.type AS type, tag_types.description AS description,
tag_types.color AS color FROM tags INNER JOIN tag_types WHERE tag LIKE ? LIMIT 7";
        public const string GetTagByTagString = @"
SELECT tags.id AS id, tags.tag AS tag, tag_types.type AS type, tag_types.description AS description,
tag_types.color AS color FROM tags INNER JOIN tag_types WHERE tag = ?";
        public const string GetTagByID = @"
SELECT tags.id AS id, tags.tag AS tag, tag_types.type AS type, tag_types.description AS description,
tag_types.color AS color FROM tags INNER JOIN tag_types WHERE id = ?";
        public const string GetDuplicatePosts = "SELECT *, IHCOMP(hash, ?) AS hdiff FROM posts WHERE rating < ? ORDER BY hdiff ASC LIMIT ?";
        public const string GetOptions = "SELECT * FROM options";
        public const string GetTagTypeByTypeName = "SELECT * FROM tag_types WHERE type = ?";
        public const string GetAliases = "SELECT * FROM aliases";
        public const string GetAliasByString = "SELECT * FROM aliases WHERE alias = ?";
        public const string GetUsernameByPublicKey = "SELECT username FROM public_keys WHERE public_key = ?";

        //Counts
        public const string GetPostCountByID = "SELECT COUNT(*) FROM posts WHERE id = ?";
        public const string GetTagCountByID = "SELECT COUNT(*) FROM tags WHERE id = ?";
        public const string GetTagCount = "SELECT COUNT(*) FROM tags";
        public const string GetUserCount = "SELECT COUNT(*) FROM users";
        public const string GetPostCount = "SELECT COUNT(*) FROM posts";

        //Deletions
        public const string DeletePostByID = "DELETE FROM posts WHERE id = ?";
        public const string DeleteTagByID = "DELETE FROM tags WHERE id = ?";
        public const string DeletePostTagsByPostID = "DELETE FROM post_tags WHERE post = ?";
        public const string DeletePostTagsByTagID = "DELETE FROM post_tags WHERE tag = ?";
        public const string DeleteUserByUsername = "DELETE FROM users WHERE username = ?";

        //Insertions - use the SQLWrapper.ExecuteInsert for the most classes
        public const string InsertPostTag = "INSERT INTO post_tags (post, tag) VALUES (?, ?)";
        public const string InsertTag = "INSERT INTO tags (tag, type_id) VALUES (?, ?)";
        public const string InsertTagWithID = "INSERT INTO tags (id, tag, type_id) VALUES (?, ?, ?)";
        public const string InsertAlias = "INSERT INTO aliases (alias, tagid) VALUES (?, ?)";

        //Updates
        public const string UpdateIncrementViewCount = "UPDATE posts SET viewcount = viewcount + 1 WHERE id = ?";
        public const string UpdateImageHash = "UPDATE posts SET hash = ? WHERE id = ?";
    }
}
