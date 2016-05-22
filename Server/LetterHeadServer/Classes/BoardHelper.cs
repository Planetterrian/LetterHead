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
        }

        private static List<LetterDefinition> letters = new List<LetterDefinition>();
        private static float totalWeight;

        static BoardHelper()
        {
            letters.Add(new LetterDefinition() { letter = 'a', weight = 2.5f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'b', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'c', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'd', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'e', weight = 2.7f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'f', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'g', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'h', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'i', weight = 2.5f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'j', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'k', weight = 0.7f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'l', weight = 1.1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'm', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'n', weight = 1.1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'o', weight = 2.2f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'p', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'q', weight = 0.2f, maxCount = 1});
            letters.Add(new LetterDefinition() { letter = 'r', weight = 1.1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 's', weight = 1.2f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 't', weight = 1.3f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'u', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'v', weight = .2f, maxCount = 2});
            letters.Add(new LetterDefinition() { letter = 'w', weight = 0.5f, maxCount = 1});
            letters.Add(new LetterDefinition() { letter = 'x', weight = .1f, maxCount = 1});
            letters.Add(new LetterDefinition() { letter = 'y', weight = 1f, maxCount = 2 });
            letters.Add(new LetterDefinition() { letter = 'z', weight = 0.1f, maxCount = 1 });

            totalWeight = letters.Sum(l => l.weight);
        }

        public static string GenerateBoard()
        {
            var letterString = "";

            var rand = new Random();

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
                        break;
                    }
                }
            }
            return letterString;
        }
    }
}