using LetterHeadServer.Models;
using LetterHeadShared;

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
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(LetterHeadServer.Models.ApplicationDbContext context)
        {
            if (!context.DailyGames.Any())
            {
                DailyGame.CreateNewDailyGame(CategoryManager.Type.Normal);
                DailyGame.CreateNewDailyGame(CategoryManager.Type.Advanced);
            }
        }
    }
}
