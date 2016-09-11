using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LetterHeadServer.Classes
{
    public static class BoardHelper
    {
        private class LetterDefinition
        {
            public char letter;
            public float weight;
            public int maxCount;

            public bool IsVowel()
            {
                return letter == 'a' || letter == 'e' || letter == 'i' || letter == 'o' || letter == 'u';
            }
        }

        private static List<LetterDefinition> letters = new List<LetterDefinition>();
        private static float totalWeight;
        public static Random rand = new Random();


        static BoardHelper()
        {
            letters.Add(new LetterDefinition() { letter = 'a', weight = 9f, maxCount = 3 });
            letters.Add(new LetterDefinition() { letter = 'b', weight = 2f, maxCount = 2 });
            letters.Add(new LetterDefinition() { letter = 'c', weight = 2f, maxCount = 2 });
            letters.Add(new LetterDefinition() { letter = 'd', weight = 4f, maxCount = 3 });
            letters.Add(new LetterDefinition() { letter = 'e', weight = 12f, maxCount = 3 });
            letters.Add(new LetterDefinition() { letter = 'f', weight = 2f, maxCount = 2 });
            letters.Add(new LetterDefinition() { letter = 'g', weight = 3f, maxCount = 3 });
            letters.Add(new LetterDefinition() { letter = 'h', weight = 2f, maxCount = 2 });
            letters.Add(new LetterDefinition() { letter = 'i', weight = 9f, maxCount = 3 });
            letters.Add(new LetterDefinition() { letter = 'j', weight = 1f, maxCount = 1 });
            letters.Add(new LetterDefinition() { letter = 'k', weight = 1f, maxCount = 1 });
            letters.Add(new LetterDefinition() { letter = 'l', weight = 4f, maxCount = 3 });
            letters.Add(new LetterDefinition() { letter = 'm', weight = 2f, maxCount = 2 });
            letters.Add(new LetterDefinition() { letter = 'n', weight = 6f, maxCount = 3 });
            letters.Add(new LetterDefinition() { letter = 'o', weight = 8f, maxCount = 3 });
            letters.Add(new LetterDefinition() { letter = 'p', weight = 2f, maxCount = 2 });
            letters.Add(new LetterDefinition() { letter = 'q', weight = 1f, maxCount = 1 });
            letters.Add(new LetterDefinition() { letter = 'r', weight = 6f, maxCount = 3 });
            letters.Add(new LetterDefinition() { letter = 's', weight = 4f, maxCount = 3 });
            letters.Add(new LetterDefinition() { letter = 't', weight = 6f, maxCount = 3 });
            letters.Add(new LetterDefinition() { letter = 'u', weight = 4f, maxCount = 3 });
            letters.Add(new LetterDefinition() { letter = 'v', weight = 2f, maxCount = 2 });
            letters.Add(new LetterDefinition() { letter = 'w', weight = 2f, maxCount = 2 });
            letters.Add(new LetterDefinition() { letter = 'x', weight = 1f, maxCount = 1 });
            letters.Add(new LetterDefinition() { letter = 'y', weight = 2f, maxCount = 2 });
            letters.Add(new LetterDefinition() { letter = 'z', weight = 1f, maxCount = 1 });

            totalWeight = letters.Sum(l => l.weight);
        }

        public static string GenerateBoard()
        {
            var letterString = "";
            var vowelCount = 0;
            
            while (letterString.Length < 10)
            {
                var arrow = (float)rand.NextDouble()*totalWeight;
                var cur = 0f;

                foreach (var letterDefinition in letters)
                {
                    cur += letterDefinition.weight;
                    if (arrow <= cur)
                    {
                        if(letterString.ToCharArray().Count(l => l == letterDefinition.letter) >= letterDefinition.maxCount)
                            break;

                        letterString += letterDefinition.letter;

                        if (letterDefinition.IsVowel())
                            vowelCount++;
                        break;
                    }
                }
            }

            // Ensure there's at least 2 vowels
            if (vowelCount < 2 || vowelCount > 7)
                letterString = GenerateBoard();

            return letterString;
        }
    }
}