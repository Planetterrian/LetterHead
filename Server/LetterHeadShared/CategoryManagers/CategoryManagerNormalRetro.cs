﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LetterHeadShared.CategoryManagers
{
    public class CategoryManagerNormalRetro : CategoryManager
    {
        public CategoryManagerNormalRetro()
        {
            categories.Add(new Category()
            {
                name = "Roll Call",
                description = "Use all letters at least once.  Worth 20 points.",
                GetScore = (words, uniqueLetterCount, existingScores) => uniqueLetterCount == 10 ? 20 : 0
            });

            categories.Add(new Category()
            {
                name = "3 & 4 Letters",
                description = "Make at least ten 3-letter and 4-letter words.  Worth 30 points.",
                GetScore = (words, uniqueLetterCount, existingScores) => words.Count(w => w.Length == 3 || w.Length == 4) >= 10 ? 30 : 0
            });

            categories.Add(new Category()
            {
                name = "5 & 6 Letters",
                description = "Make at least five 5-letter and 6-letter words.  Worth 30 points.",
                GetScore = (words, uniqueLetterCount, existingScores) => words.Count(w => w.Length == 5 || w.Length == 6) >= 5 ? 30 : 0
            });

            categories.Add(new Category()
            {
                name = "20 Words",
                description = "Make at least 20 words.  Worth 40 points.",
                GetScore = (words, uniqueLetterCount, existingScores) => words.Count >= 20 ? 40 : 0
            });

            categories.Add(new Category()
            {
                name = "Upper Bonus",
                description = "Successfully complete all of the above categories for an additional 35 points.",
                alwaysActive = true,
                GetScore = (words, uniqueLetterCount, existingScores) =>
                {
                    var score = 35;
                    for (var i = 0; i < 4; i++)
                    {
                        if (existingScores[i] == 0)
                        {
                            score = 0;
                            break;
                        }
                    }

                    return score;
                }
            });

            categories.Add(new Category()
            {
                name = "Starter",
                description = "Make at least 5 words that start with the same letter.  Worth 25 points.",
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

                    if (leterDict.Any(letterDictKV => letterDictKV.Value >= 5))
                    {
                        return 25;
                    }

                    return 0;
                }
            });

            categories.Add(new Category()
            {
                name = "Small Straight",
                description = "Make at least one 3-letter, 4-letter, and 5-letter word.  Worth 30 points.",
                GetScore = (words, uniqueLetterCount, existingScores) => words.Count(w => w.Length == 3) > 0 && words.Count(w => w.Length == 4) > 0 &&
                 words.Count(w => w.Length == 5) > 0 ? 30 : 0
            });

            categories.Add(new Category()
            {
                name = "Large Straight",
                description = "Make at least one 3-letter, 4-letter, 5-letter, and 6-letter letter word.  Worth 40 points.",
                GetScore = (words, uniqueLetterCount, existingScores) => words.Count(w => w.Length == 3) > 0 && words.Count(w => w.Length == 4) > 0 &&
                 words.Count(w => w.Length == 5) > 0 && words.Count(w => w.Length == 6) > 0  ? 40 : 0
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