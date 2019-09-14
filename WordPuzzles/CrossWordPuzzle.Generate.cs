using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WD_toolbox.Maths.Space;
using WDLang.Words;
using WD_toolbox;
using WD_toolbox.Maths.Range;

namespace WordPuzzles.WordPuzzles
{
    public partial class CrossWordPuzzle
    {
        //----------------------------------------------------------------------------------
        // Creating crosswords
        //----------------------------------------------------------------------------------
        public void Generate(LanguageDictionary dictionary, int seed = -1)
        {
            int maxWords = TargetWordCount;
            seeedRandomGenerator(seed);
            this.Dictionary = dictionary;
            //start with a long word

            Word word = getNextWord(maxWords);
            string text = word.PrimarySpelling.ToLower();
            WordVector vec = new WordVector(new Point(roundUpTillEven((Width - text.Length) / 2),
                                                      roundUpTillEven(Height / 2)),
                                            Dir2D.Right,
                                            text.Length);

            vec.Word = word;
            grid.placeWord(text, vec);
            addUsedWord(text);
            this.words.Add(vec);
            dump(words);

            int esc = 0;
            //place Word
            while ((WordCount < maxWords) && (word != null))
            {
                word = getNextWord(maxWords);
                if (word == null)
                {
                    break; //done
                }

                //temp, escape 
                esc++;
                if (esc > 10000)
                {
                    break;
                }

                text = word.PrimarySpelling.ToLower();

                List<Tuple<WordVector, int>> possibleVecs = GetPossibleVectors(text);

                //remove any rule violations
                possibleVecs = possibleVecs.Where(V => !this.Words.Any(N => !canTheseTwoVectorsExistTogether(N, V.Item1))).ToList();

                if (possibleVecs.Count > 0)
                {
                    //we want the most intersectiony option
                    int maxIntersects = possibleVecs.Max(V => V.Item2);
                    possibleVecs = possibleVecs.Where(V => V.Item2 == maxIntersects).ToList();

                    vec = possibleVecs.GetRandomItem(rnd).Item1;
                    vec.Word = word;
                    grid.placeWord(text, vec);
                    addUsedWord(text);
                    this.words.Add(vec);
                    dump(words);

                    esc = 0;
                }
            }

            //finish up the related tasks
            AssignIndexes();
        }

        private int roundUpTillEven(int i)
        {
            return ((i % 2) == 1) ? (i + 1) : i;
        }

        private List<Tuple<WordVector, int>> GetPossibleVectors(string text)
        {
            List<Dir2D> dirs = new List<Dir2D>() { Dir2D.Right, Dir2D.Down };

            List<Tuple<WordVector, int>> possibleVecs = new List<Tuple<WordVector, int>>();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    foreach (Dir2D dir in dirs)
                    {
                        WordVector vec = new WordVector(new Point(x, y), dir, text.Length);

                        int intersectionCount = testPlacement(text, vec);

                        if (intersectionCount > 0)
                        {
                            possibleVecs.Add(new Tuple<WordVector, int>(vec, intersectionCount));
                        }
                    }
                }
            }

            return possibleVecs;
        }

        private int testPlacement(string text, WordVector vec)
        {
            bool ok = true;
            int intersectionCount = 0;
            if ((vec.LastX >= Width) || (vec.LastX < 0) || (vec.LastY >= Height) || (vec.LastY < 0))
            {
                return 0; //placement leaves grid
            }

            Point pos = vec.Pos;
            for (int i = 0; i < vec.Length; i++)
            {
                char? charGrid = grid[pos.X, pos.Y];
                char charText = text[i];
                if (charGrid != null)
                {
                    if (charGrid.Value == charText)
                    {
                        intersectionCount++;
                    }
                    else
                    {
                        ok = false;
                        break;
                    }
                }

                //next pos
                pos = pos.NextPoint(vec.Dir);
            }

            return ok ? intersectionCount : 0;
        }


        private List<WordVector> GetPossibleVectors_old(string text)
        {
            List<WordVector> possibleVecs = new List<WordVector>();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    //is there a character at the grid which matches our text
                    char? _c = grid[x, y];
                    if ((_c != null) && (text.Contains(_c.Value)))
                    {
                        List<WordVector> interSectingVecsToCurrentGridPoint = (from v in Words where v.Intersects(x, y) select v).ToList();

                        //only one word should intersect this letter (so far)
                        //we are trying to place this word orthogonally across it
                        if (interSectingVecsToCurrentGridPoint.Count == 1)
                        {
                            WordVector intersect = interSectingVecsToCurrentGridPoint[0];
                            Dir2D newDir = otherDir(intersect.Dir);
                            Dir2D newDirOrthogonal = intersect.Dir;

                            //check all possible intersection points
                            foreach (int pos in text.IndexOfAll(_c.Value))
                            {
                                //create the vector that would position the word orthogonally across the other
                                WordVector newVec;
                                if (newDir == Dir2D.Down)
                                {
                                    newVec = new WordVector(new Point(x, y - pos), newDir, text.Length);
                                }
                                else //across
                                {
                                    newVec = new WordVector(new Point(x - pos, y), newDir, text.Length);
                                }

                                //check it works (does not put a different letter on top of another word)
                                if (!this.grid.AnyConflicts(newVec, text))
                                {
                                    if (words.TrueForAll(W => canTheseTwoVectorsExistTogether(W, newVec)))
                                    {
                                        possibleVecs.Add(newVec);
                                    }


                                    /*
                                    //get the orthogonal words intersected
                                    List<WordVector> interSectingVecsToNewVec = (from v in Words where v.Intersects(newVec) select v).ToList();

                                    //can't intersect another WordVector in the same direction
                                    if (interSectingVecsToNewVec.TrueForAll(V => V.Dir == newDirOrthogonal))
                                    {
                                        //the word should not impinge on exclusion zones of the existing vectors
                                        //other than the one it intersects orthogonally to
                                        List<WordVector> nonIntersectingVectors = Words.Where(V => !interSectingVecsToNewVec.Contains(V)).ToList();

                                        if (nonIntersectingVectors.TrueForAll(V => !(newVec.InCrosswordStyleExclusionZoneOF(V))))
                                        {
                                            possibleVecs.Add(newVec);
                                        }
                                    }*/

                                }
                            }
                        }
                    }
                }
            }

            //done
            return possibleVecs;
        }


        public bool canTheseTwoVectorsExistTogether(WordVector a, WordVector b)
        {
            if (a.isVertical && b.isVertical)
            {
                return _canTheseHorizontalVectorsExistTogether(Transpose(a), Transpose(b));
            }
            else if (a.isHorizontal && b.isHorizontal)
            {
                return _canTheseHorizontalVectorsExistTogether(a, b);
            }
            else if (a.isHorizontal && b.isVertical)
            {
                return _canTheseOrthogonalVectorsExistTogether(a, b);
            }
            else
            {
                return _canTheseOrthogonalVectorsExistTogether(b, a);
            }
        }


        public bool _canTheseOrthogonalVectorsExistTogether(WordVector horiz, WordVector vert)
        {
            //this code is non obvious, NOTE:
            // A) orthogonal vectors often intersect. They are supposed to in a crossword
            // B) All intersections (letters are equal) are already approved.

            switch (Othogonal_Rule)
            {
                case WordPlacement_Othogonal_Rules.CanTouch:
                    return true;

                case WordPlacement_Othogonal_Rules.NoTouch:
                    //also this statements are non obvious (looks wrong) till you do the working out
                    if ((horiz.Y >= vert.Y) && (horiz.Y <= vert.LastY))
                    {
                        return ((horiz.X + horiz.Length) != vert.X) &&
                                (horiz.X != (vert.X + 1));
                    }

                    if ((vert.X >= horiz.X) && (vert.X <= horiz.LastX))
                    {
                        return ((vert.Y + vert.Length) != horiz.Y) &&
                                (vert.Y != (horiz.Y + 1));
                    }

                    return true;

                default:
                    return true;
            }
        }

        public bool _canTheseHorizontalVectorsExistTogether(WordVector _a, WordVector _b)
        {
            //check this to eliminate many checks later
            if ((Parallel_Rule == WordPlacement_Parallel_Rules.EvenLinesOnly) &&
              (((_a.Y % 2) == 1) || ((_b.Y % 2) == 1)))
            {
                return false;
            }

            //a is the left most
            bool swap = (_a.Pos.X > _b.Pos.X);
            WordVector a = swap ? _b : _a;
            WordVector b = swap ? _a : _b;

            RangeInt aRange = RangeInt.FromStartAndLength(a.Pos.X, a.Length);
            RangeInt bRange = RangeInt.FromStartAndLength(b.Pos.X, b.Length);

            //rule out vectors not near each other to eliminate many checks later
            RangeInt aRangePlus = RangeInt.FromStartAndLength(a.Pos.X, a.Length + 1);
            RangeInt bRangePlus = RangeInt.FromStartAndLength(b.Pos.X, b.Length + 1);
            if (!aRangePlus.Intersects(bRangePlus))
            {
                return true; //no reason to test
            }

            if (a.Pos.Y == b.Pos.Y)
            {
                //run on rules apply
                switch (RunOnRule)
                {
                    case WordPlacement_RunOn_Rules.NoRestrictions:
                        return true;
                    case WordPlacement_RunOn_Rules.NoContainment:
                        return !(aRange.Contains(bRange) || bRange.Contains(aRange));
                    case WordPlacement_RunOn_Rules.NoRunOnOverLap:
                        return !aRange.Intersects(bRange);
                    case WordPlacement_RunOn_Rules.NoRunOnTouching:
                        return false;  //because of previous tests we know they must at least but together
                    default:
                        throw new InvalidOperationException();
                }
            }
            else if (Math.Abs(a.Pos.Y - b.Pos.Y) == 1)
            {
                //parallel rules apply
                switch (Parallel_Rule)
                {
                    case WordPlacement_Parallel_Rules.NoRestrictions:
                        return true;
                    case WordPlacement_Parallel_Rules.MustHaveGap:
                        return false;
                    case WordPlacement_Parallel_Rules.EvenLinesOnly:
                        return true; //previous testing has already solved the case for false
                    default:
                        throw new InvalidOperationException();
                }
            }
            else
            {
                //they are not near each other vertically
                return true;
            }

        }

        private WordVector Transpose(WordVector v)
        {
            return new WordVector(new Point(v.Pos.Y, v.Pos.X), otherDir(v.Dir), v.Length);
        }

        protected static Dir2D otherDir(Dir2D dir)
        {
            return (dir == Dir2D.Right) ? Dir2D.Down : Dir2D.Right;
        }

        private Word getNextWord(int maxWords)
        {
            double perDone = WordCount / (double)maxWords;
            int minWordLen = 3;// getMinLen(perDone);
            int maxWordLen = 10;// getMaxWordLen(perDone, minWordLen);

            Word wrd = GetRandomUnusedWord(minWordLen, maxWordLen);
            /*
            //relax constraints till a word is found
            while ((wrd == null) && (minWordLen > 3))
            {
                minWordLen--;
                GetRandomUnusedWord(minWordLen - 2, minWordLen);
            }*/
            return wrd;
        }

        /*private int getMinLen(double perDone)
        {
            int minWordLen = Width / 5;
            if (perDone < 0.1)
            {
                minWordLen = (int)(Width / 2.5);
            }
            else if (perDone < 0.2)
            {
                minWordLen = (int)(Width / 3.5);
            }
            minWordLen = Math.Max(3, minWordLen);
            minWordLen = Math.Min(Width-3, minWordLen);
            return minWordLen;
        }

        private int getMaxWordLen(double perDone, int minWordLen)
        {
            int maxWordLen = Width;
            if (perDone > 0.9)
            {
                maxWordLen = (int)(Width / 3);
            }
            maxWordLen = Math.Max(minWordLen + 2, maxWordLen);
            maxWordLen = Math.Min(Width - 1, maxWordLen);
            return maxWordLen;
        }*/

        public Dir2D randomCrossWordDir()
        {
            return rnd.nextBool() ? Dir2D.Right : Dir2D.Down;
        }

  
    }
}
