using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WDLang.Words;
using WD_toolbox;
using WD_toolbox.Files;

namespace WordPuzzles.WordPuzzles
{
    public class WordPuzzle
    {
        [NonSerialized]
        private LanguageDictionary dic;

        public LanguageDictionary Dictionary
        {
            get { return dic; }
            set { dic = value; resetDictionaryRandomAcessLut(); }
        }

        private HashSet<string> usedWords = new HashSet<string>();

        [NonSerialized]
        protected Random rnd = new Random();

        [NonSerialized]
        //an array to provide unOrdered traversal of the dictionary
        private int[] dicRandomAcessLut;

        public WordPuzzle()
        {
        }

        private void resetDictionaryRandomAcessLut()
        {
            Random lutRnd = new Random(0xface);

            //setup an array to provide unOrdered traversal of the dictionary
            dicRandomAcessLut = new int[dic.Count];
            for (int i = 0; i < dic.Count; i++)
            {
                dicRandomAcessLut[i] = i;
            }
            dicRandomAcessLut.Shuffle(lutRnd);
        }

        /// <summary>
        /// resets the random number generator, use -1 to use time
        /// </summary>
        /// <param name="seed"></param>
        public void seeedRandomGenerator(int seed)
        {
            rnd = (seed == -1) ? new Random() : new Random(seed);
        }

        protected Word GetRandomUnusedWordByWildCardPattern(string wildcard)
        {
            return GetRandomUnusedWord(W => Wildcards.isMatch(W.PrimarySpelling, wildcard, false));
        }
        protected Word GetRandomUnusedWordByWildCardPattern(int minLen, int maxLen, string wildcard)
        {
            return GetRandomUnusedWord(W => (W.PrimarySpelling.Length >= minLen) && 
                                            (W.PrimarySpelling.Length <= maxLen) &&
                                            Wildcards.isMatch(W.PrimarySpelling, wildcard, false));
        }
        //random word finding
        protected Word GetRandomUnusedWord(int minLen, int maxLen)
        {
            return GetRandomUnusedWord(W => (W.PrimarySpelling.Length >= minLen) && (W.PrimarySpelling.Length <= maxLen));
        }

        protected Word GetRandomUnusedWord(Predicate<Word> test)
        {
            int startPos = rnd.Next(dic.Count);

            //search whole dictionary, offset by pos
            for (int i = 0; i < dic.Count; i++)
            {
                //transform i to a randomly offset position (startPos)
                int pos = (i + startPos) % dic.Count;
                //pseudo-random 1 to 1 map of the position to promote equal probability of items being selected
                pos = dicRandomAcessLut[pos];
                
                Word word = dic[pos];
                if (test(word) && !isUsedWord(word.PrimarySpelling))
                {
                    return word;
                }
            }

            //the whole dictionary has been traversed
            return null;
        }

        protected string normaliseForUsedWordList(string s)
        {
            return s.Trim().ToLower();
        }


        //----------------------------------------------------------------------------------
        // Used Words
        //----------------------------------------------------------------------------------
        public bool isUsedWord(string word)
        {
            string nomWord = normaliseForUsedWordList(word);
            return usedWords.Contains(nomWord);
        }

        public void addUsedWord(string word)
        {
            if (!isUsedWord(word))
            {
                usedWords.Add(normaliseForUsedWordList(word));
            }
        }

        public bool removeUsedWord(string word)
        {
            string nomWord = normaliseForUsedWordList(word);
            return usedWords.Remove(nomWord);
        }

        public void clearUsedWords()
        {
            usedWords.Clear();
        }
    }
}
