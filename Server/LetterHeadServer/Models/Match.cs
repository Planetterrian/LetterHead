using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LetterHeadServer.Models
{
    public class Match
    {
        public int Id { get; set; }

        [Index]
        public List<User> Users { get; set; }
        public User Winner { get; set; }

        [Index]
        public State CurrentState { get; set; }

        [Index]
        public DailyGame DailyGame { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? StartedOn { get; set; }
        public string Letters { get; set; }

        public enum State
        {
            Pregame, Running, Ended
        }

        public static Match New()
        {
            return new Match()
                   {
                       CurrentState = State.Pregame,
                       CreatedOn = DateTime.Now,
                   };
        }

        public Dictionary<object, object> DTO()
        {
            return new Dictionary<object, object>
                   {
                    {"CurrentState", CurrentState},
                    {"Users", Users.Select(u => u.Username).ToList()},
                    {"CurrentState", CurrentState},
                    {"Letters", Letters},
                   };
        }

        public static Match GetById(ApplicationDbContext db, int matchId)
        {
            return db.Matches.Find(matchId);
        }

        public bool UserAuthorized(User currentUser)
        {
            return Users.Any(u => u.Id == currentUser.Id);
        }
    }
}