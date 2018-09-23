using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LetterHeadShared.CategoryManagers
{
    public class CategoryManagerAdvanced : CategoryManager
    {
        public CategoryManagerAdvanced()
        {
            categories.Add(new Category()
            {
                name = "Roll Call",
                description = "1 point for the first time each letter used.  Max 10 points.  Goal: Use all 10 letters!",
                GetScore = (words, uniqueLetterCount, existingScores) => uniqueLetterCount * 1
            });

            categories.Add(new Category()
            {
                name = "3 & 4 Letters",
                description = "1 point for each 3-letter and 4-letter word.  No max.  Goal: 15 words.",
                GetScore = (words, uniqueLetterCount, existingScores) => words.Count(w => w.Length == 3 || w.Length == 4) * 1
            });

            categories.Add(new Category()
            {
                name = "5 & 6 Letters",
                description = "2 points for each 5-letter and 6-letter word.  No max.  Goal: 10 words.",
                GetScore = (words, uniqueLetterCount, existingScores) => words.Count(w => w.Length == 5 || w.Length == 6) * 2
            });

            categories.Add(new Category()
            {
                name = "Word Count",
                description = "1 point for each word.  No max.  Goal: 20 words.",
                GetScore = (words, uniqueLetterCount, existingScores) => words.Count * 1
            });

            categories.Add(new Category()
            {
                name = "Upper Bonus",
                description = "Score at least 65 points in the above categories to earn and additional 35 points.",
                alwaysActive = true,
                GetScore = (words, uniqueLetterCount, existingScores) =>
                {
                    var upperScore = 0;
                    for (var i = 0; i < 4; i++)
                    {
                        upperScore += existingScores[i];
                    }

                    if (upperScore >= 65)
                        return 35;

                    return 0;
                }
            });

            categories.Add(new Category()
            {
                name = "Starter",
                description = "Make at least 10 words that start with the same letter.  Worth 25 points.",
                GetScore = (words, uniqueLetterCount, existingScores) =>
                {
                    var leterDict = new Dictionary<char, int>();
                    foreach (var word in words)
                    {
                        if (leterDict.ContainsKey(word[0]))
                        {
                            leterDict[word[0]] = leterDict[word[0]] + 1;
                        }
                        else
                        {
                            leterDict[word[0]] = 1;
                        }
                    }

                    if (leterDict.Any(letterDictKV => letterDictKV.Value >= 10))
                    {
                        return 25;
                    }

                    return 0;
                }
            });

            categories.Add(new Category()
            {
                name = "Small Straight",
                description = "Make at least one 3-letter, 4-letter, 5-letter, and 6-letter word.  Worth 30 points.",
                GetScore = (words, uniqueLetterCount, existingScores) => words.Count(w => w.Length == 3) > 0 && words.Count(w => w.Length == 4) > 0 &&
                 words.Count(w => w.Length == 5) > 0 && words.Count(w => w.Length == 6) > 0 ? 30 : 0
            });

            categories.Add(new Category()
            {
                name = "Large Straight",
                description = "Make at least one 3-letter, 4-letter, 5-letter, 6-letter, and 7-letter letter word.  Worth 40 points.",
                GetScore = (words, uniqueLetterCount, existingScores) => words.Count(w => w.Length == 3) > 0 && words.Count(w => w.Length == 4) > 0 &&
                 words.Count(w => w.Length == 5) > 0 && words.Count(w => w.Length == 6) > 0 && words.Count(w => w.Length == 7) > 0 ? 40 : 0
            });

            categories.Add(new Category()
            {
                name = "Fast Fingers",
                description = "Score based on word length. 3-4 = 1 point, 5-6 = 2 points, 7+ letters = 3 points.",
                GetScore = (words, uniqueLetterCount, existingScores) => (words.Count(w => w.Length == 3 || w.Length == 4) * 1) +
                 (words.Count(w => w.Length == 5 || w.Length == 6) * 2) +
                 (words.Count(w => w.Length >= 7) * 3)
            });

            categories.Add(new Category()
            {
                name = "7+ Letters",
                description = "Make 7+ letter words for 25 points each with a max of 75 points for this category.",
                GetScore = (words, uniqueLetterCount, existingScores) =>
                {
                    var score = words.Count(w => w.Length >= 7) * 25;
                    if (score > 75)
                        score = 75;

                    return score;
                }
            });

            categories.Add(new Category()
            {
                alwaysActive = true,
                name = "Big Word Bonus",
                description = "Each word with at least 8 letters is worth 50 points.  No max!",
                GetScore = (words, uniqueLetterCount, existingScores) => words.Count(w => w.Length >= 8) * 50
            });

        }

    }
}