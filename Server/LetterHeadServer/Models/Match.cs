using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using MyWebApplication;

namespace LetterHeadServer.Models
{
    public class Match
    {
        public int Id { get; set; }

        [Index]
        public virtual List<User> Users { get; set; }
        public virtual User Winner { get; set; }

        [Index]
        public LetterHeadShared.DTO.Match.State CurrentState { get; set; }

        [Index]
        public virtual DailyGame DailyGame { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? StartedOn { get; set; }
        public string Letters { get; set; }


        public static Match New()
        {
            return new Match()
                   {
                       CurrentState = LetterHeadShared.DTO.Match.State.Pregame,
                       CreatedOn = DateTime.Now,
                   };
        }

        public LetterHeadShared.DTO.Match DTO()
        {
            return Startup.Mapper.Map<LetterHeadShared.DTO.Match>(this);
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