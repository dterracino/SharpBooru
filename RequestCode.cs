namespace TA.SharpBooru
{
    //DeleteResource
    //EditResource
    //AddResource

    //SearchImg
    //AddAlias

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

        //In: string Pattern
        //Out: List<ulong> IDs
        Search = 20,

        //In: byte[] ImageHash
        //Out: List<ulong> IDs
        Search_Img = 21,

        //In: string Username
        //In: string Password
        //Out: BooruUser User
        Login = 22,
    }
}
