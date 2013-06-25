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
            
            booru.Save();
        }
    }
}