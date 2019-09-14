using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WordPuzzles
{
    public class CrossWordOptions
    {
        public int Width  {get; set;}
        public int Height { get; set; }

        public CrossWordOptions(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }
    }
}
