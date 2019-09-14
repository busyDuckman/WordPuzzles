using System;
using System.Drawing;
using WD_toolbox;
using WD_toolbox.Maths.Space;

namespace WordPuzzles.WordPuzzles
{
    public interface IWordGrid
    {
        int Width { get; }
        int Height { get; }
        System.Drawing.Size Size { get; }

        char? this[System.Drawing.Point p] { get; set; }
        char? this[int x, int y] { get; set; }

        void ClearGrid();        

        bool InBounds(Point p);
        bool InBounds(int x, int y);
    }

    public static class IWordGridExtensions
    {
        public static bool VectorHasLetters(this IWordGrid g, WordVector vec)
        {
            Point p = vec.Pos;
            for (int i = 0; i < vec.Length; i++)
            {
                if (g[p] == null)
                {
                    return false;
                }

                p = p.NextPoint(vec.Dir);
            }

            return true;
        }

        public static string GetWord(this IWordGrid g, WordVector vec)
        {
            if (vec.Dir == Dir2D.None)
            {
                return null;
            }

            Point p = vec.Pos;
            int len = 0;
            string str = "";

            while (g.InBounds(p) && (g[p] != null) && (len<vec.Length))
            {
                str = str + g[p].Value;
                len++;
                p = p.NextPoint(vec.Dir);
            }

            return (len == vec.Length) ? str : null;
        }

        /// <summary>
        /// Returns the longest possible vector that occupies valid letters in the grid
        /// </summary>
        /// <param name="pos">Start pos</param>
        /// <param name="dir">Direction</param>
        /// <returns>null id no vector is possible</returns>
        public static WordVector FindLongestWordVector(this IWordGrid g, Point pos, Dir2D dir)
        {
            if(dir == Dir2D.None)
            {
                return null;
            }

            Point p = pos;
            int len = 0;

            while (g.InBounds(p) && (g[p] != null))
            {
                len++;
                p = p.NextPoint(dir);
            }

            return (len > 0) ? new WordVector(pos, dir, len) : null;
        }

        /// <summary>
        /// Can this word be placed along the vector with out altering any existing letters?
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool AnyConflicts(this IWordGrid g, WordVector vec, string str)
        {
            if (vec.Dir == Dir2D.None)
            {
                return true;
            }

            Point p = vec.Pos;
            int len = 0;

            while (g.InBounds(p) && 
                  (len < vec.Length) &&
                  isSameLetterOrNull(g[p], str[len])
                  )
            {
                len++;
                p = p.NextPoint(vec.Dir);
            }

            return (len-1) == vec.Length;
        }

        private static bool isSameLetterOrNull(char? a, char b)
        {
            if (a != null)
            {
                return isSameLetter(a.Value, b);
            }
            return true;
        }

        private static bool isSameLetter(char c, char p)
        {
            return (char.ToLower(c) == char.ToLower(p));
        }

        public static void placeWord(this IWordGrid g, string text, WordVector vec)
        {
            if (vec.Dir == Dir2D.None)
            {
                return;
            }

            Point p = vec.Pos;
            int len = 0;

            while (g.InBounds(p) &&
                  (len < vec.Length))
            {
                g[p] = text[len];
                len++;
                p = p.NextPoint(vec.Dir);
            }
        }
    }
}
