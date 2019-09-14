using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WD_toolbox.Maths.Range;
using WD_toolbox.Maths.Space;
using WD_toolbox;

namespace WordPuzzles.WordPuzzles
{
    public class WordVector : ICloneable
    {
        public Point Pos { get; protected set; }
        public Dir2D Dir { get; protected set; }
        public int Length { get; protected set; }

        public bool isHorizontal {get {return (Dir == Dir2D.Right) | (Dir == Dir2D.Left);}}
        public bool isVertical {get {return (Dir == Dir2D.Down) | (Dir == Dir2D.Up);}}
        public int X { get { return Pos.X; } }
        public int Y { get { return Pos.Y; } }

        public int LastX 
        { 
            get 
            {
                if(Length == 0) { 
                    return Pos.X;
                }
                return (Dir == Dir2D.Left) ? (Pos.X - Length + 1) :
                       ( (Dir == Dir2D.Right) ? (Pos.X + Length - 1) : Pos.X );
  
            } 
        }
        public int LastY { 
            get 
            {
                if(Length == 0) { 
                    return Pos.Y;
                }
                return (Dir == Dir2D.Up) ? (Pos.Y - Length + 1) :
                       ( (Dir == Dir2D.Down) ? (Pos.Y + Length - 1) : Pos.Y );
  
            } 
        }

        public WordVector(Point pos, Dir2D dir, int length)
        {
            this.Pos = pos;
            this.Dir = dir;
            this.Length = length;
        }

        protected WordVector(WordVector other)
        {
            this.Pos = other.Pos;
            this.Dir = other.Dir;
            this.Length = other.Length;
        }

        public bool Intersects(Point p)
        {
            return Intersects(p.X, p.Y);
        }

        public bool Intersects(int x, int y)
        {
            switch (Dir)
            {
                case Dir2D.None:
                    return (Pos.X == x) && (Pos.Y == y);
                case Dir2D.Up:
                    return (Pos.X == x) && (y > (Pos.Y-Length))  && (y <= Pos.Y);
                case Dir2D.Down:
                    return (Pos.X == x) && (y < (Pos.Y+Length))  && (y >= Pos.Y);
                case Dir2D.Left:
                    return (Pos.Y == y) && (x > (Pos.X-Length))  && (x <= Pos.X);
                case Dir2D.Right:
                    return (Pos.Y == y) && (x < (Pos.X+Length))  && (x >= Pos.X);
                default:
                    return false;
            }
        }

        private int resolveAsIndex(int x, int y)
        {
            switch (Dir)
            {
                case Dir2D.None:
                    if((Pos.X == x) && (Pos.Y == y))
                    {
                        return 0;
                    }
                    break;
                case Dir2D.Up:
                    if((Pos.X == x) && (y > (Pos.Y - Length)) && (y <= Pos.Y))
                    {
                        return Pos.Y - y;
                    }
                    break;
                case Dir2D.Down:
                    if((Pos.X == x) && (y < (Pos.Y + Length)) && (y >= Pos.Y))
                    {
                        return y - Pos.Y;
                    }
                    break;
                case Dir2D.Left:
                    if((Pos.Y == y) && (x > (Pos.X - Length)) && (x <= Pos.X))
                    {
                        return Pos.X - x;   
                    }
                    break;
                case Dir2D.Right:
                    if((Pos.Y == y) && (x < (Pos.X + Length)) && (x >= Pos.X))
                    {
                        return x - Pos.X;
                    }
                    break;
            }
            return -1;
        }

        public char? this[Point p]
        {
            get { return this[p.X, p.Y]; }
        }

        public char? this[int x, int y]
        {
            get
            {
                if (Word != null)
                {
                    int pos = resolveAsIndex(x, y);
                    if (pos >= 0)
                    {
                        return this.Word.PrimarySpelling[pos];
                    }
                }
                return null;
            }
        }

        internal bool Intersects(WordVector other)
        {
            //TODO: simple algorithm, needs to be replaced with something more efficient
            Point p = Pos;
            for (int i = 0; i < Length; i++)
            {
                if (other.Intersects(p))
                {
                    return true;
                }

                p = p.NextPoint(Dir);
            }
            return false;
            /*
            if(this.Dir == Dir2D.None) {
                return other.Intersects(this.Pos);
            }
            else if(other.Dir == Dir2D.None) {
                return this.Intersects(other.Pos);
            }
            else if(isHorizontal(this.Dir))
            {
                if(isHorizontal(other.Dir))
                {
                    //Range.
                }
                else
                {
                }
            }
            else //this is vertical
            {
            }
            */
        }


        /*internal bool InCrosswordStyleExclusionZoneOF(WordVector v)
        {
            // exclusion zone
            //    
            //   ####                                         ####
            //  #word#   is treaded as a rec and two points  -####-
            //   ####                                         ####

            /*
            bool vIsHoriz = isHorizontal(v.Dir);
            bool thisIsHoriz = isHorizontal(v.Dir);

            RangeInt boxHoriz = vIsHoriz ? RangeInt.FromStartAndLength(v.Pos.X, v.Length) :
                                           RangeInt.FromStartAndLength(v.Pos.X - 1, 3);

            RangeInt boxVert = vIsHoriz ? RangeInt.FromStartAndLength(v.Pos.Y - 1, 3) :
                                          RangeInt.FromStartAndLength(v.Pos.Y, v.Length);

            RangeInt thisHoriz = thisIsHoriz ? RangeInt.FromStartAndLength(Pos.X, Length) :
                                               RangeInt.FromStartAndLength(Pos.X, 1);

            RangeInt thisVert = thisIsHoriz ? RangeInt.FromStartAndLength(Pos.Y, 1) :
                                              RangeInt.FromStartAndLength(Pos.Y, Length);

            if (boxHoriz.Intersects(thisHoriz) && boxVert.Intersects(thisVert))
            {
                return true;
            }

            for(


            return (this.Intersects(v.Pos.NextPoint(v.Dir, -1)) || 
                    this.Intersects(v.Pos.NextPoint(v.Dir, v.Length)));

        }*/


        public object Clone()
        {
            return new WordVector(this);
        }

        public WDLang.Words.Word Word { get; set; }

        public int IndexValue { get; set; }

    }
}
