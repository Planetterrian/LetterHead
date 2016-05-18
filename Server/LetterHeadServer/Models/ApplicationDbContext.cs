using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace LetterHeadServer.Models
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Models.User> Users { get; set; }
        public DbSet<Models.DailyGame> DailyGames { get; set; }
        public DbSet<Models.Match> Matches { get; set; }
        public DbSet<Models.MatchRound> MatchRounds { get; set; }
        
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public static ApplicationDbContext Get()
        {
            if (HttpContext.Current == null)
                return new ApplicationDbContext();

            var context = HttpContext.Current.Items[typeof(ApplicationDbContext).AssemblyQualifiedName] as ApplicationDbContext;

            if (context == null)
            {
                context = new ApplicationDbContext();
                HttpContext.Current.Items[typeof(ApplicationDbContext).AssemblyQualifiedName] = context;
            }

            return context;
        }
    }
}