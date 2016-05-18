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
        public LetterHeadShared.DTO.Match.MatchState CurrentState { get; set; }

        [Index]
        public virtual DailyGame DailyGame { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? StartedOn { get; set; }
        public string Letters { get; set; }
        public int RoundTimeSeconds { get; set; }

        public virtual List<MatchRound> Rounds { get; set; }
        public int CurrentRoundNumber { get; set; }

        public static Match New(ApplicationDbContext db, List<User> users, int roundCount)
        {
            var match = new Match()
                   {
                       CurrentState = LetterHeadShared.DTO.Match.MatchState.Pregame,
                       CreatedOn = DateTime.Now,
                       RoundTimeSeconds = 120,
                       Rounds = new List<MatchRound>(),
                       Users = users,
                   };

            db.Matches.Add(match);
            db.SaveChanges();

            foreach (var user in users)
            {
                match.AddRounds(user, roundCount);
            }

            db.SaveChanges();

            return match;
        }

        public MatchRound CurrentRoundForUser(User user)
        {
            return Rounds.FirstOrDefault(r => r.User.Id == user.Id && r.Number == CurrentRoundNumber);
        }

        private void AddRounds(User user, int roundCount)
        {
            for (int i = 0; i < roundCount; i++)
            {
                var round = new MatchRound()
                            {
                                Match = this,
                                User = user,
                                CurrentState = MatchRound.RoundState.NotStarted,
                                Number = i,
                                Words = new List<string>()
                            };

                Rounds.Add(round);
            }
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