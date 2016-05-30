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
        public bool DoOverUsed { get; set; }
        public string Letters { get; set; }

        public List<string> Words { get; set; }
        public string WordsJoined
        {
            get
            {
                return string.Join(",", Words);
            }
            set
            {
                if(string.IsNullOrEmpty(value))
                    Words = new List<string>();
                else
                    Words = value.Split(',').ToList();
            }
        }

        public int UsedLetterIds { get; set; }
        public string CategoryName { get; set; }
        public LetterHeadShared.DTO.MatchRound.RoundState CurrentState { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? ActivatedOn { get; set; }

        public MatchRound()
        {
            Words = new List<string>();
        }
        

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
                Score = CalculateScore(Words, NumberOfSetBits(UsedLetterIds), ExistingCategoryScores());
                CurrentState = LetterHeadShared.DTO.MatchRound.RoundState.Ended;
                Match.AdvanceRound();
            }
            else
            {
                CurrentState = LetterHeadShared.DTO.MatchRound.RoundState.WaitingForCategory;
            }

            if (User.MostWords < Words.Count)
            {
                User.MostWords = Words.Count;
            }
        }

        private List<int> ExistingCategoryScores()
        {
            var scores = new int[Startup.CategoryManager.Categories.Count];

            var rounds = Match.Rounds.Where(m => m.User.Id == User.Id && m.Number < Match.CurrentRoundNumber);
            foreach (var round in rounds)
            {
                if(string.IsNullOrEmpty(round.CategoryName))
                    continue;

                var categoryIndex = Startup.CategoryManager.GetCategoryIndex(round.CategoryName);
                scores[categoryIndex] = round.Score;
            }

            return scores.ToList();
        }

        public int CalculateScore(List<string> words, int uniqueLetters, List<int> existingCategoryScores)
        {
            var category = Startup.CategoryManager.GetCategory(CategoryName);

            return category.GetScore(words, uniqueLetters, existingCategoryScores);
        }

        public void SetCategory(ApplicationDbContext db, Category category)
        {
            CategoryName = category.name;

            if (CurrentState == LetterHeadShared.DTO.MatchRound.RoundState.WaitingForCategory)
            {
                End();
            }

            db.SaveChanges();
        }

        private int NumberOfSetBits(int i)
        {
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }

    }
}