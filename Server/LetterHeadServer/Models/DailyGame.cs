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
        public string Letters { get; set; }
        public DateTime StartDate;
        public DateTime? EndDate;

        public static void CreateNewDailyGame()
        {
            var db = ApplicationDbContext.Get();

            var current = Current();
            current?.End(db);

            db.DailyGames.Add(new DailyGame()
                                                      {
                                                          StartDate = DateTime.Now,
                                                          Letters = BoardHelper.GenerateBoard()
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
                game.Terminate();
            }

            context.SaveChanges();
        }

        public Match CreateMatchForUser(ApplicationDbContext context, User currentUser)
        {
            var match = Match.New(context, new List<User>(){ currentUser }, 10);
            match.DailyGame = this;
            match.CurrentState = LetterHeadShared.DTO.Match.MatchState.Pregame;
            match.Letters = Letters;

            context.SaveChanges();

            match.AddRounds(context);

            context.SaveChanges();

            return match;
        }
    }
}