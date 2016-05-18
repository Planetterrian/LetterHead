using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LetterHeadShared.DTO
{
    public class MatchRound
    {
        public int Id;
        public int Number;
        public int Score;
        public int UserId;
        public List<string> Words;
        public string CategoryName;
        public Match.MatchState CurrentState;

        public Category Category(CategoryManager categoryManager)
        {
            return categoryManager.GetCategory(CategoryName);
        }
    }
}
