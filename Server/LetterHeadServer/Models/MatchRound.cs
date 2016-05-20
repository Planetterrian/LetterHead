using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LetterHeadShared;
using MyWebApplication;

namespace LetterHeadServer.Models
{
    public class MatchRound
    {
        public int Id { get; set; }
        public virtual Match Match { get; set; }
        public virtual User User { get; set; }
        public int Number { get; set; }
        public int Score { get; set; }
        public List<string> Words { get; set; }
        public string CategoryName { get; set; }
        public LetterHeadShared.DTO.MatchRound.RoundState CurrentState { get; set; }
        public DateTime? StartedOn { get; set; }

        public Category Category(CategoryManager categoryManager)
        {
            return categoryManager.GetCategory(CategoryName);
        }

        public LetterHeadShared.DTO.MatchRound DTO()
        {
            return Startup.Mapper.Map<LetterHeadShared.DTO.MatchRound>(this);
        }

        public DateTime EndTime()
        {
            return StartedOn.Value.AddSeconds(Match.RoundTimeSeconds);
        }

        public void End()
        {
            if (!string.IsNullOrEmpty(CategoryName))
            {
                CurrentState = LetterHeadShared.DTO.MatchRound.RoundState.Ended;
                Match.AdvanceRound();
            }
            else
            {
                CurrentState = LetterHeadShared.DTO.MatchRound.RoundState.WaitingForCategory;
            }
        }
        // Returns error message or null
        public string CalculateScore(List<string> words, int uniqueLetters, Category category)
        {
            var rounds = Match.UserRounds(User);
            if (rounds.Any(r => r.CategoryName == category.name))
            {
                return "That category has already been used";
            }

            Score = category.GetScore(words, uniqueLetters);
            CategoryName = category.name;

            return null;
        }
    }
}