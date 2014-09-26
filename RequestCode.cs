﻿namespace TA.SharpBooru
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

        Get_User = 7,

        //In: string Pattern
        //Out: List<ulong> IDs
        Search_String = 20,

        //In: byte[] ImageHash
        //Out: List<ulong> IDs
        Search_Image = 21,

        //In: string Username
        //In: string Password
        Login = 22,

        //In: BooruPost Post
        //In: BooruTagList Tags
        //In: BooruImage Image
        //Out: ulong ID
        Add_Post = 30,

        //Add_Alias = 31

        //Edit_XXX = 40
        
        //Delete_XXX = 50
    }
}
