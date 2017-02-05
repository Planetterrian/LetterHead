using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LetterHeadShared.CategoryManagers;

namespace LetterHeadShared
{
    public abstract class CategoryManager
    {
        public enum Type
        {
            Legacy, Normal, Advanced, Expert
        }

        protected List<Category> categories = new List<Category>();


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

        public static CategoryManager GetManagerForScoringType(Type scoringType)
        {
            if (scoringType == Type.Legacy)
                return new CategoryManagerLegacy();
            if (scoringType == Type.Normal)
                return new CategoryManagerNormal();
            else if (scoringType == Type.Advanced)
                return new CategoryManagerAdvanced();
            else if (scoringType == Type.Expert)
                return new CategoryManagerExpert();

            return null;
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
