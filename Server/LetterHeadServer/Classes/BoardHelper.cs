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
            letters.Add(new LetterDefinition() { letter = 'a', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'b', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'c', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'd', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'e', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'f', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'g', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'h', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'i', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'j', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'k', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'l', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'm', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'n', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'o', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'p', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'q', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'r', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 's', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 't', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'u', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'v', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'w', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'x', weight = 1f, maxCount = 4});
            letters.Add(new LetterDefinition() { letter = 'y', weight = 1f, maxCount = 4 });
            letters.Add(new LetterDefinition() { letter = 'z', weight = 1f, maxCount = 4 });

            totalWeight = letters.Sum(l => l.weight);
        }

        public static string GenerateBoard()
        {
            var letterString = "";

            var rand = new Random();

            for (int i = 0; i < 10; i++)
            {
                var arrow = (float)rand.NextDouble()*totalWeight;
                var cur = 0f;

                foreach (var letterDefinition in letters)
                {
                    cur += letterDefinition.weight;
                    if (arrow <= cur)
                    {
                        letterString += letterDefinition.letter;
                    }
                }
            }
            return letterString;
        }
    }
}