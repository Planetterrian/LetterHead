using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LetterHeadShared
{
    public class CategoryManager
    {
        private List<Category> categories = new List<Category>();

        // Use this for initialization
        public CategoryManager()
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
                description = "Make at least 15 3-letter and 4-letter words.  Worth 30 points.",
                GetScore = (words, uniqueLetterCount, existingScores) => words.Count(w => w.Length == 3 || w.Length == 4) >= 15 ? 30 : 0
            });

            categories.Add(new Category()
            {
                name = "5 & 6 Letters",
                description = "Make at least 10 5-letter and 6-letter words.  Worth 30 points.",
                GetScore = (words, uniqueLetterCount, existingScores) => words.Count(w => w.Length == 5 || w.Length == 6) >= 10 ? 30 : 0
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

        public List<Category> Categories
        {
            get { return categories; }
            private set { categories = value; }
        }

        public Category GetCategory(string name)
        {
            return categories.First(c => c.name == name);
        }

        public int GetCategoryIndex(string categoryName)
        {
            for (int index = 0; index < Categories.Count; index++)
            {
                var category = Categories[index];
                if (category.name == categoryName)
                    return index;
            }

            return -1;
        }
    }

    public class Category
    {
        public string name;
        public string description;
        public bool alwaysActive = false;

        /// <summary>
        /// Word list, Unique Letter Count, List of existing scores. Returns score
        /// </summary>
        public Func<List<string>, int, List<int>,  int> GetScore;

    }
}
