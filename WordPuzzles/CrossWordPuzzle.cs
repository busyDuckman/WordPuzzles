using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WD_toolbox;
using WD_toolbox.Data.DataStructures;
using WD_toolbox.Maths.Range;
using WD_toolbox.Maths.Space;
using WD_toolbox.Rendering;
using WD_toolbox.Validation;
using WDLang.Words;

namespace WordPuzzles.WordPuzzles
{
    public partial class CrossWordPuzzle : WordPuzzle, IValidatable
    {
        //--------------------------------------------------------------------------------------------
        // Enums and internal classes
        //--------------------------------------------------------------------------------------------
        public enum WordPlacement_RunOn_Rules
        {
            NoRestrictions,     // HamburGerman
            NoContainment,      // pANTs
            NoRunOnOverLap,     // HamburgerGerman
            NoRunOnTouching     // Hamburger German
        };

        public enum WordPlacement_Parallel_Rules
        {
            NoRestrictions,
            MustHaveGap,
            EvenLinesOnly
        }

        public enum WordPlacement_Othogonal_Rules
        {
            CanTouch,
            NoTouch
        }

        //--------------------------------------------------------------------------------------------
        // Instance Data
        //--------------------------------------------------------------------------------------------
        WordGrid grid;
        private List<WordVector> words;

        //--------------------------------------------------------------------------------------------
        // Accessors
        //--------------------------------------------------------------------------------------------
        public int Width { get { return grid.Width; } }
        public int Height { get { return grid.Height; } }
        public int TargetWordCount { get; set; }

        public IReadOnlyList<WordVector> Words
        {
            get { return words.AsReadOnly(); }
        }

        public int WordCount { get { return words.Count; } }

        public WordPlacement_RunOn_Rules RunOnRule { get; set; }
        public WordPlacement_Parallel_Rules Parallel_Rule { get; set; }
        public WordPlacement_Othogonal_Rules Othogonal_Rule { get; set; }

        public bool ViewSolution { get; set; }

        //--------------------------------------------------------------------------------------------
        // Constructors and factory methods
        //--------------------------------------------------------------------------------------------
        protected CrossWordPuzzle(int width, int height) : base()
        {
            ViewSolution = true;

            grid = WordGrid.FromSize(width, height);
            words = new List<WordVector>();

            TargetWordCount = 15;

            //rules
            RunOnRule = WordPlacement_RunOn_Rules.NoRunOnTouching;
            Parallel_Rule = WordPlacement_Parallel_Rules.MustHaveGap;
            Othogonal_Rule = WordPlacement_Othogonal_Rules.NoTouch;

            initTransientData();
        }

        private void initTransientData()
        {
            grid.customiseCell = (X, Y, C) => customizeCell(X, Y, C);
        }

        private WordGrid.GridRenderSetting customizeCell(int x, int y, char? c)
        {
            bool isNotNull = (c != null);
            WordGrid.GridRenderSetting gs = new WordGrid.GridRenderSetting();
            Point p = new Point(x, y);
            gs.guideText = getIndexString(p);

            gs.fillColor = isNotNull ? Color.White : Color.LightGray;

            gs.outlineStyle = isNotNull ? WordGrid.OutlineStyleEnum.Square : WordGrid.OutlineStyleEnum.NoOutline;
            gs.outlineColor = Color.Blue;

            if (isNotNull && (!char.IsLetter(c.Value)))
            {
                //show the - type things that are not letters
                gs.letterVisible = true;
                gs.letterColor = Color.Black;
            }
            else
            {
                gs.letterColor = Color.Red;
                gs.letterVisible = this.ViewSolution;
            }

            return gs;
        }

        protected bool IsValid()
        {
            return (grid != null);
        }

        public static CrossWordPuzzle FromSize(int width, int height)
        {
            if ((width < 6) || (height < 6))
            {
                return null;
            }

            CrossWordPuzzle puzzle = new CrossWordPuzzle(width, height);
            return puzzle.IsValid() ? puzzle : null;
        }

        //----------------------------------------------------------------------------------
        // Generating clues
        //----------------------------------------------------------------------------------
        public string generateClues()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Across");
            foreach (WordVector wv in
                    from wrd in Words
                    where (wrd.Dir == Dir2D.Right)
                    orderby wrd.IndexValue
                    select wrd)
            {
                sb.AppendLine(string.Format("{0}: ({1})\t{2}", wv.IndexValue, wv.Word.TypeAbreviation, wv.Word.ShortDefinition));
                //sb.AppendLine();//"\t" + wv.Word.Definitions.First().TheDefinition);
            }

            sb.AppendLine();
            sb.AppendLine("Down");
            foreach (WordVector wv in
                    from wrd in Words
                    where (wrd.Dir == Dir2D.Down)
                    orderby wrd.IndexValue
                    select wrd)
            {
                sb.AppendLine(string.Format("{0}: ({1})\t{2}", wv.IndexValue, wv.Word.TypeAbreviation, wv.Word.ShortDefinition));
                //sb.AppendLine();//"\t" + wv.Word.Definitions.First().TheDefinition);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Assigns 1 across etc
        /// </summary>
        private void AssignIndexes()
        {
            var acrossWords = from wrd in Words
                    where (wrd.Dir == Dir2D.Right)
                    orderby wrd.Pos.Y, wrd.Pos.X
                    select wrd;

            var downWords = from wrd in Words
                    where (wrd.Dir == Dir2D.Down)
                    orderby wrd.Pos.Y, wrd.Pos.X
                    select wrd;

            int i = 1;
            foreach (WordVector wv in acrossWords)
            {
                wv.IndexValue = i;
                i++;
            }

            foreach (WordVector wv in downWords)
            {
                WordVector other = acrossWords.FirstOrDefault(W => W.Pos == wv.Pos);
                if (other == null)
                {
                    wv.IndexValue = i;
                    i++;
                }
                else
                {
                    wv.IndexValue = other.IndexValue; //ie 15 across..., 15 down...
                }
            }
        }

       
        //----------------------------------------------------------------------------------
        // Rendering
        //----------------------------------------------------------------------------------
        public void Render(IRenderer r, Rectangle bounds)
        {
            grid.Render(r, bounds);
        }

        private string getIndexString(Point p)
        {
            var wrd = words.FirstOrDefault(W => W.Pos == p);
            if (wrd != null)
            {
                return wrd.IndexValue.ToString();
            }
            return null;
        }

        public void Clear()
        {
            grid.ClearGrid();
            words.Clear();
            clearUsedWords();
        }

        /// <summary>
        /// Dumpt to a plain text file (for debug purposes)
        /// </summary>
        public string dump(List<WordVector> words)
        {
            string[] lines = new string[this.Height];
            for (int i = 0; i < this.Height; i++)
            {
                lines[i] = new string(' ', this.Width);
            }

            //how a character is positioned on the string array
            Func<char, char> caps = C => ("" + C).ToUpper()[0];
            Func<char, char> low = C => ("" + C).ToLower()[0];
            Func<char, char, char> setChar = (N, C) => (N == ' ') ? low(C) : ((low(C) == low(N)) ? caps(C) : '%');
            Action<int, int, char> place = (X, Y, C) => lines[Y] = lines[Y].AlterCharAt(X, N => setChar(N, C));

            //fill in the word
            foreach (var word in words)
            {
                //the word, or **** for wordles vectors
                string text = (word.Word != null) ? word.Word.PrimarySpelling : new string('*', word.Length);
                
                Point pos = word.Pos;
                for(int i=0; i< word.Length; i++)
                {
                    char c1 = text[i];
                    char c2 = word[pos].Value;

                    char outPut = (c1 != c2) ? '!' : c1;

                    place(pos.X, pos.Y, outPut);

                    //next pos
                    pos = pos.NextPoint(word.Dir);
                }
            }

            //return result
            //Misc.Dump(lines.ListAll("\n"));
            return lines.ListAll("\n");
        }

        //--------------------------------------------------------------------------------------------
        // IValidatable
        //--------------------------------------------------------------------------------------------
        public Why Valid()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    //is there a character at the grid which matches our text
                    char? _c = grid[x, y];
                    if (_c != null)
                    {
                        foreach (WordVector wv in words)
                        {
                            if (wv.Intersects(x, y))
                            {
                                char? letter = wv[x, y];
                                if (letter == null)
                                {
                                    return Why.FalseBecause("Code error: WordVector.Intersects(x, y) is not meshing properly with WordVector[x, y]");
                                }

                                if (letter.Value != _c.Value)
                                {
                                    return Why.FalseBecause("word / grid letter mismatch. ({0}, {1}) is {2}, but intersects a {3} in '{4}'",
                                                            x, y, _c.Value,
                                                            letter.Value, wv.Word.PrimarySpelling);
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
