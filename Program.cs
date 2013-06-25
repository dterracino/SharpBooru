using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TEAM_ALPHA.SharpBooru
{
    public class Program
    {
        static void Main(string[] args)
        {
            Booru booru = Booru.Load("C:\\SharpBooru");
            booru.Add(new BooruPost()
            {
                Image = new BooruImage("C:\\Users\\Admin\\desktop\\image.jpg"),
                ViewCount = 1,
                EditCount = 1,
                Comment = "SEX VOLL GEIL",
                CompareImage = null,
                ID = 1,
                Private = false,
                CreationDate = DateTime.Now,
                Size = new System.Drawing.Size(1754, 1274),
                Rating = 2,
                Score = 1000,
                Source = "MY FUCKING DESKTOP",
                Thumbnail = null,
                Tags = new List<BooruTag>()
                {
                    new BooruTag("bikini"),
                    new BooruTag("thighhighs")
                }
            });
            booru.Save();
        }
    }
}