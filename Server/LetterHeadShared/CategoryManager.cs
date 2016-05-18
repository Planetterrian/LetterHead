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
                description = "Use all letters at least once",
                GetScore = (words, uniqueLetterCount) => uniqueLetterCount == 10 ? 20 : 0
            });

            categories.Add(new Category()
            {
                name = "3 & 4 Letters",
                description = "",
                GetScore = (words, uniqueLetterCount) => words.Count(w => w.Length == 3 || w.Length == 4) >= 15 ? 30 : 0
            });

            categories.Add(new Category()
            {
                name = "5 & 6 Letters",
                description = "",
                GetScore = (words, uniqueLetterCount) => words.Count(w => w.Length == 5 || w.Length == 6) >= 10 ? 30 : 0
            });

            categories.Add(new Category()
            {
                name = "20 Words",
                description = "",
                GetScore = (words, uniqueLetterCount) => words.Count >= 20 ? 40 : 0
            });

            categories.Add(new Category()
            {
                name = "Upper Bonus",
                description = "",
                GetScore = (words, uniqueLetterCount) =>
                {
                    var score = 35;
                    for (var i = 0; i < 4; i++)
                    {
                        if (categories[i].GetScore(words, uniqueLetterCount) == 0)
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
                description = "",
                GetScore = (words, uniqueLetterCount) =>
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
                description = "",
                GetScore = (words, uniqueLetterCount) => words.Count(w => w.Length == 3) > 0 && words.Count(w => w.Length == 4) > 0 &&
                 words.Count(w => w.Length == 5) > 0 && words.Count(w => w.Length == 6) > 0 ? 30 : 0
            });

            categories.Add(new Category()
            {
                name = "Large Straight",
                description = "",
                GetScore = (words, uniqueLetterCount) => words.Count(w => w.Length == 3) > 0 && words.Count(w => w.Length == 4) > 0 &&
                 words.Count(w => w.Length == 5) > 0 && words.Count(w => w.Length == 6) > 0 && words.Count(w => w.Length == 7) > 0 ? 40 : 0
            });

            categories.Add(new Category()
            {
                name = "Weighted Words",
                description = "",
                GetScore = (words, uniqueLetterCount) => (words.Count(w => w.Length == 3 || w.Length == 4) * 1) +
                 (words.Count(w => w.Length == 5 || w.Length == 6) * 2) +
                 (words.Count(w => w.Length >= 7) * 3)
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
    }

    public class Category
    {
        public string name;
        public string description;

        public Func<List<string>, int, int> GetScore;

    }
}
