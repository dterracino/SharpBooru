namespace TA.SharpBooru
{
    //Login
    //GetResource
    //GetAllTags
    //DeleteResource
    //EditResource
    //AddResource
    //Search
    //Resource
    //StringList
    //ULongList
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

        //In: string Pattern
        //Out: List<ulong> IDs
        Search = 20
    }
}
