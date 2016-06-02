using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Hangfire;
using LetterHeadServer.Classes;
using LetterHeadServer.Controllers;
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
        public bool ShieldUsed { get; set; }
        public bool StealTimeUsed { get; set; }
        public bool StealLetterUsed { get; set; }
        public string LetterStolen { get; set; }

        public float StealTimeDelay { get; set; }
        public float StealLetterDelay { get; set; }

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

        public string EndRoundJobId { get; set; }
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
            return Match.CategoryScores(User);
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

        public float TimeRemaining()
        {
            return (float) ((EndTime() - DateTime.Now).TotalSeconds);
        }

        public void Start(ApplicationDbContext db)
        {
            CurrentState = LetterHeadShared.DTO.MatchRound.RoundState.Active;
            StartedOn = DateTime.Now;
            ScheduleRoundEnd();

            if (StealTimeDelay > 0)
            {
                // Steal Time
                ScheduleStealTime(db);
            }
            if (StealLetterDelay > 0)
            {
                // Steal Time
                ScheduleStealLetter(db);
            }
        }

        public async Task ScheduleStealTime(ApplicationDbContext db)
        {
            await Task.Delay((int)(StealTimeDelay * 1000));

            var matchRtm = RealTimeMatchManager.GetMatch(Match.Id);

            matchRtm.AddMessage(new RealTimeMatch.Message(controller =>
            {
                controller.StartStealTime();
            }));

            StartedOn = StartedOn.Value.AddSeconds(-20);

            ScheduleRoundEnd();
            db.SaveChanges();
        }

        public async Task ScheduleStealLetter(ApplicationDbContext db)
        {
            await Task.Delay((int)(StealLetterDelay * 1000));

            var matchRtm = RealTimeMatchManager.GetMatch(Match.Id);

            var letterToSteal = BoardHelper.rand.Next(0, 10);
            LetterStolen = Letters.Substring(letterToSteal, 1);

            matchRtm.AddMessage(new RealTimeMatch.Message(controller =>
            {
                controller.StartStealLetter(LetterStolen);
            }));

            var newLetters = Letters.Substring(0, letterToSteal);
            if (letterToSteal < 9)
                newLetters += Letters.Substring(letterToSteal + 1, Letters.Length - (letterToSteal + 1));

            Letters = newLetters;

            db.SaveChanges();
        }
        public void ScheduleRoundEnd()
        {
            if(!string.IsNullOrEmpty(EndRoundJobId))
                BackgroundJob.Delete(EndRoundJobId);

            EndRoundJobId = BackgroundJob.Schedule(() => new MatchController().ManuallyEndRound(Id, Id), EndTime());
        }
    }
}