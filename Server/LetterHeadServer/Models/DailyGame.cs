using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LetterHeadServer.Classes;

namespace LetterHeadServer.Models
{
    public class DailyGame
    {
        public int Id { get; set; }
        public string LettersEncoded { get; set; }
        public DateTime StartDate;
        public DateTime? EndDate;

        public string RoundLetters(int roundNumber)
        {
            return LettersEncoded.Split(',')[roundNumber];
        }

        private const int RoundCount = 10;

        public static void CreateNewDailyGame()
        {
            var db = ApplicationDbContext.Get();

            var current = Current();
            current?.End(db);

            var letterArray = new List<string>();

            for (int i = 0; i < RoundCount; i++)
            {
                letterArray.Add(BoardHelper.GenerateBoard());
            }


            db.DailyGames.Add(new DailyGame()
            {
                StartDate = DateTime.Now,
                LettersEncoded =  String.Join(",", letterArray)
            });

            db.SaveChanges();
        }

        public static DailyGame Current()
        {
            var db = ApplicationDbContext.Get();

            return db.DailyGames.OrderByDescending(d => d.Id).FirstOrDefault();
        }

        private void End(ApplicationDbContext context)
        {
            EndDate = DateTime.Now;
            context.SaveChanges();

            EndExistingGames(context);
        }

        private void EndExistingGames(ApplicationDbContext context)
        {
            var games = context.Matches.Where(m => m.DailyGame.Id == Id).ToList();

            foreach (var game in games)
            {
                game.EndMatch();
            }

            context.SaveChanges();
        }

        public Match CreateMatchForUser(ApplicationDbContext context, User currentUser)
        {
            var match = Match.New(context, new List<User>(){ currentUser }, RoundCount);
            match.DailyGame = this;
            match.CurrentState = LetterHeadShared.DTO.Match.MatchState.Pregame;
                        context.SaveChanges();

            match.AddRounds(context);

            context.SaveChanges();

            var roundsList = match.Rounds.ToList();
            for (int index = 0; index < roundsList.Count; index++)
            {
                var round = roundsList[index];
                round.Letters = RoundLetters(index);
            }
            context.SaveChanges();

            return match;
        }
    }
}