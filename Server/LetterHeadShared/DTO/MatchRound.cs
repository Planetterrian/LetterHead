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
        public int UsedLetterIds;
        public List<string> Words;
        public string CategoryName;
        public RoundState CurrentState;
        public DateTime? StartedOn;
        public string Letters;

        public bool DoOverUsed;
        public bool ShieldUsed;
        

        public enum RoundState
        {
            NotStarted, Active, Ended, WaitingForCategory
        }

        public Category Category(CategoryManager categoryManager)
        {
            return categoryManager.GetCategory(CategoryName);
        }
    }
}
