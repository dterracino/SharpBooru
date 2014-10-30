namespace TA.SharpBooru
{
    public enum RequestCode
    {
        //In: ulong ID
        //Out: BooruPost Post
        Get_Post = 0,

        //In: ulong ID
        //Out: BooruImage Thumb
        Get_Thumb = 1,

        //In: ulong ID
        //Out: BooruImage Img
        Get_Image = 2,

        //In: ulong ID
        //Out: BooruTag Tag
        Get_Tag = 3,

        //Out: BooruInfo Info
        Get_Info = 4,

        //Out: List<string> AllTags
        Get_AllTags = 5,

        //In: ulong ID
        //Out: BooruTagList Tags
        Get_PostTags = 6,

        //Out: BooruUser User
        Get_User = 7,

        //Out: uint PostCount
        Get_PostCount = 8,

        //In: string Pattern
        //Out: List<ulong> IDs
        Search_String = 20,

        //In: byte[] ImageHash
        //Out: List<ulong> IDs
        Search_Image = 21,

        //In: string Username
        //In: string Password
        Login = 22,

        //void
        Logout = 23,

        //In: string Term
        //Out: List<BooruTag> Tags
        Search_Tags = 24,

        //void
        Start_GC = 25,

        //In: BooruPost Post
        //In: BooruTagList Tags
        //In: BooruImage Image
        //Out: ulong ID
        Add_Post = 30,

        //Add_Alias = 31

        //In: BooruPost Post
        //In: BooruTagList Tags
        Edit_Post = 40,

        //In: ulong ID
        //In: BooruImage Image
        Edit_Image = 41,

        //Edit_Tag

        //In: ulong ID
        Delete_Post = 50,

        //Delete_Tag
        //Delete_User

        //No need to send bytes after Disconnect
        //Disconnect instantly terminates the connection
        Disconnect = 65535
    }
}
