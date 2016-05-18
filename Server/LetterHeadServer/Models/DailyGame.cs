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
        }

        public Match CreateMatchForUser(ApplicationDbContext context, User currentUser)
        {
            var match = Match.New(context, new List<User>(){ currentUser }, 1);
            match.DailyGame = this;
            match.Letters = Letters;

            context.SaveChanges();

            return match;
        }
    }
}