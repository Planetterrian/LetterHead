using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Hangfire;
using LetterHeadServer.Controllers;
using LetterHeadShared.DTO;
using MyWebApplication;

namespace LetterHeadServer.Models
{
    public class User
    {
        public User()
        {
            Friends = new HashSet<User>();
        }

        public int Id { get; set; }
        public int MostWords { get; set; }

        public string Email { get; set; }
        public string PasswordHash { get; set; }

        [Index]
        [MaxLength(32)]
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
        public string FacebookPictureUrl { get; set; }
        public string DeviceGUID { get; set; }
        public string FacebookId { get; set; }
        public string FacebookToken { get; set; }
        public DateTime SignupDate { get; set; }

        public string AndroidNotificationToken { get; set; }
        public string IosNotificationToken { get; set; }


        public virtual ICollection<Match> Matches { get; set; }
        public virtual ICollection<Invite> Invites { get; set; }
        public virtual ICollection<User> Friends { get; set; }


        [Index]
        [MaxLength(64)]
        public string SessionId { get; set; }

        public void GenerateNewSessionId()
        {
            SessionId = Guid.NewGuid().ToString();
        }

        public Match GetMatch(ApplicationDbContext db, DailyGame dailyGame)
        {
            return db.Matches.FirstOrDefault(m => m.Users.Any(u => u.Id == Id) && m.DailyGame != null && m.DailyGame.Id == dailyGame.Id);
        }

        public UserInfo DTO()
        {
            return Startup.Mapper.Map<UserInfo>(this);
        }

        public UserStats Stats(ApplicationDbContext db)
        {
            var stats = new UserStats();
            stats.gamesWon = Matches.Count(m => m.Winner != null && m.Winner.Id == Id);
            stats.gamesLost = Matches.Count(m => m.Winner != null && m.Winner.Id != Id);
            stats.gamesPlayed = stats.gamesWon + stats.gamesLost;

            var rounds = db.MatchRounds.Where(m => m.User.Id == Id);

            if (rounds.Any())
            {
                stats.averageScore = (int)(rounds.Sum(r => r.Score)/rounds.Count());
                stats.bestScore = rounds.Max(r => r.Score);
            }
            else
            {
                stats.averageScore = 0;
                stats.bestScore = 0;
            }
            stats.mostWords = MostWords;

            return stats;
        }

        public void SendNotification(NotificationDetails message)
        {
            if(string.IsNullOrEmpty(AndroidNotificationToken) && string.IsNullOrEmpty(IosNotificationToken))
                return;

            BackgroundJob.Enqueue(() => new BackendController().SendNotification(Id, message));
        }
    }
}