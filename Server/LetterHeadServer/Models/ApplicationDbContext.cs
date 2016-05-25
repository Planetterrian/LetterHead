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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Match>().
                HasMany(c => c.Users).
                WithMany(p => p.Matches).
                Map(
                    m =>
                    {
                        m.MapLeftKey("MatchId");
                        m.MapRightKey("UserId");
                        m.ToTable("MatchUsers");
                    });

            modelBuilder.Entity<User>().HasMany(u => u.Friends).WithMany().Map(f =>
            {
                f.ToTable("UserFriends");
                f.MapLeftKey("User");
                f.MapRightKey("Friend");
            });

            modelBuilder.Entity<User>().HasMany(u => u.Invites).WithRequired(i => i.User);

            modelBuilder.Entity<Match>()
        .HasMany(a => a.Rounds).
        WithRequired()
        .WillCascadeOnDelete(true);
        }
    }
}