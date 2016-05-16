using LetterHeadServer.Models;

namespace LetterHeadServer.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<LetterHeadServer.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(LetterHeadServer.Models.ApplicationDbContext context)
        {
            if (!context.DailyGames.Any())
            {
                DailyGame.CreateNewDailyGame();
            }
        }
    }
}
