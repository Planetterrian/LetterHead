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
            Settings_Music = true;
            Settings_ClearWords = true;
            Settings_Sound = true;
            Settings_Notifications = true;
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
        public string LostPasswordToken { get; set; }
        public DateTime SignupDate { get; set; }
        public DateTime? LastFreePowerup { get; set; }
        public int HighScoreMatchId { get; set; }

        public int Stat_GamesPlayedNoResigner { get; set; }
        public int Stat_GamesWon { get; set; }
        public int Stat_GamesLost { get; set; }
        public int Stat_GamesTied { get; set; }
        public int Stat_BestScore { get; set; }
        public int Stat_TotalScore { get; set; }

        public bool Settings_Music { get; set; }
        public bool Settings_Sound { get; set; }
        public bool Settings_ClearWords { get; set; }
        public bool Settings_Notifications { get; set; }

        public string AndroidNotificationToken { get; set; }
        public string IosNotificationToken { get; set; }
        public bool IsPremium { get; set; }


        public virtual ICollection<Match> Matches { get; set; }
        public virtual ICollection<Invite> Invites { get; set; }
        public virtual ICollection<User> Friends { get; set; }


        public List<int> PowerupCountList { get; set; }
        public string PowerupCountListJoined
        {
            get
            {
                if (PowerupCountList == null)
                    return "";

                return string.Join(",", PowerupCountList);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    PowerupCountList = new List<int>() { 2, 2, 2, 2};
                else
                    PowerupCountList = value.Split(',').Select(int.Parse).ToList();
            }
        }
        
        [Index]
        [MaxLength(64)]
        public string SessionId { get; set; }


        public int PowerupCount(LetterHeadShared.Powerup.Type powerupType)
        {
            if ((int)powerupType >= PowerupCountList.Count)
                return 0;

            return PowerupCountList[(int)powerupType];
        }

        public void ConsumePowerup(LetterHeadShared.Powerup.Type powerupType)
        {
            PowerupCountList[(int)powerupType]--;
        }

        public void AddPowerup(LetterHeadShared.Powerup.Type powerupType, int qty)
        {
            PowerupCountList[(int)powerupType] += qty;
        }

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

            stats.gamesWon = Stat_GamesWon;
            stats.gamesLost = Stat_GamesLost;
            stats.gamesTied = Stat_GamesTied;
            stats.gamesPlayed = Stat_GamesWon + Stat_GamesLost + Stat_GamesTied;

            if (Stat_GamesPlayedNoResigner > 0)
                stats.averageScore = Stat_TotalScore/Stat_GamesPlayedNoResigner;
            else
                stats.averageScore = 0;

            stats.bestScore = Stat_BestScore;
            stats.mostWords = MostWords;

            return stats;
        }

        public void SendNotification(NotificationDetails message)
        {
            if(string.IsNullOrEmpty(AndroidNotificationToken) && string.IsNullOrEmpty(IosNotificationToken))
                return;

            BackgroundJob.Enqueue(() => new BackendController().SendNotification(Id, message));
        }

        public bool CanDoDailyPowerup()
        {
            if (!LastFreePowerup.HasValue)
                return true;

            var timeUtc = DateTime.UtcNow;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);

            return easternTime.Date > LastFreePowerup.Value;
        }
    }
}