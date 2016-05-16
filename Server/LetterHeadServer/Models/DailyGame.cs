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
        public DateTime EndDate;

        public static void CreateNewDailyGame()
        {
            var db = ApplicationDbContext.Get();
            db.DailyGames.Add(new DailyGame()
                                                      {
                                                          StartDate = DateTime.Now,
                                                          Letters = BoardHelper.GenerateBoard()
                                                      });


            db.SaveChanges();
        }
    }
}