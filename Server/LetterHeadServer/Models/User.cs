using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using LetterHeadShared.DTO;
using MyWebApplication;

namespace LetterHeadServer.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Email { get; set; }
        public string PasswordHash { get; set; }

        [Index]
        [MaxLength(32)]
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
        public string DeviceGUID { get; set; }
        public DateTime SignupDate { get; set; }

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
    }
}