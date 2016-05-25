using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Hangfire;
using LetterHeadServer.Classes;
using MyWebApplication;

namespace LetterHeadServer.Models
{
    public class Match
    {
        public int Id { get; set; }

        [Index]
        public virtual List<User> Users { get; set; }

        public List<int> ClearedUserIds { get; set; }

        public string ClearedUserIdsJoined
        {
            get
            {
                return string.Join(",", ClearedUserIds);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    ClearedUserIds = new List<int>();
                else
                    ClearedUserIds = value.Split(',').Select(int.Parse).ToList();
            }
        }


        [Index]
        public virtual User Winner { get; set; }

        [Index]
        public LetterHeadShared.DTO.Match.MatchState CurrentState { get; set; }

        [Index]
        public virtual DailyGame DailyGame { get; set; }

        public virtual User CurrentUserTurn { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? StartedOn { get; set; }
        public int RoundTimeSeconds { get; set; }
        public int MaxRounds { get; set; }

        public virtual ICollection<MatchRound> Rounds { get; set; }
        public int CurrentRoundNumber { get; set; }

        public static Match New(ApplicationDbContext db, List<User> users, int roundCount)
        {
            var roundTime = 120;

            if (Environment.UserName == "Pete")
                roundTime = 30;

                var match = new Match()
                   {
                       CurrentState = LetterHeadShared.DTO.Match.MatchState.WaitingForPlayers,
                       CreatedOn = DateTime.Now,
                       RoundTimeSeconds = roundTime,
                       Rounds = new List<MatchRound>(),
                       Users = users,
                        MaxRounds = roundCount,
                };

            if(users.Count > 0)
                match.CurrentUserTurn = users[0];

            db.Matches.Add(match);
            db.SaveChanges();
            
            return match;
        }

        public void AddRounds(ApplicationDbContext db)
        {
            foreach (var user in Users)
            {
                AddUserRounds(user, MaxRounds);
            }

            db.SaveChanges();
        }

        public MatchRound CurrentRoundForUser(User user)
        {
            return Rounds.FirstOrDefault(r => r.User.Id == user.Id && r.Number == CurrentRoundNumber);
        }

        private void AddUserRounds(User user, int roundCount)
        {
            for (int i = 0; i < roundCount; i++)
            {
                var round = new MatchRound()
                            {
                                Match = this,
                                User = user,
                                CurrentState = LetterHeadShared.DTO.MatchRound.RoundState.NotStarted,
                                Number = i,
                                Words = new List<string>()
                            };

                Rounds.Add(round);
            }
        }

        public LetterHeadShared.DTO.Match DTO(bool sparse = false)
        {
            var dto = Startup.Mapper.Map<LetterHeadShared.DTO.Match>(this);
            if (sparse)
                dto.Rounds = null;

            return dto;
        }

        public static Match GetById(ApplicationDbContext db, int matchId)
        {
            return db.Matches.Find(matchId);
        }

        public bool UserAuthorized(User currentUser)
        {
            return Users.Any(u => u.Id == currentUser.Id);
        }

        public void AdvanceRound()
        {
            var currentUserIndex = Users.IndexOf(CurrentUserTurn);
            currentUserIndex++;
            if (currentUserIndex >= Users.Count)
            {
                EndRound();
            }
            else
            {
                CurrentRoundForUser(CurrentUserTurn).CurrentState = LetterHeadShared.DTO.MatchRound.RoundState.Ended;
                CurrentUserTurn = Users[currentUserIndex];
            }
        }

        private void EndRound()
        {
            if (CurrentRoundNumber == Rounds.Count - 1)
            {
                EndMatch();
                return;
            }

            CurrentUserTurn = Users[0];
            CurrentRoundNumber++;
            GenerateRandomBoard();
        }

        public List<MatchRound> UserRounds(User user)
        {
            return Rounds.Where(r => r.User.Id == user.Id).ToList();
        }

        public void RandomizeUsers()
        {
            CurrentUserTurn = Users[0];
        }

        public List<int> GetScores()
        {
            var scores = new List<int>();

            for (int index = 0; index < Users.Count; index++)
            {
                var user = Users[index];
                var score = Rounds.Where(r => r.User.Id == user.Id).Sum(r => r.Score);
                scores.Add(score);
            }

            return scores;
        }

        public void EndMatch()
        {
            CurrentState = LetterHeadShared.DTO.Match.MatchState.Ended;

            DetermineWinner();
        }

        private void DetermineWinner()
        {
            if (DailyGame != null)
            {
                Winner = Users[0];
                return;
            }

            var scores = Rounds.GroupBy(r => r.User).Select(g => new {User = g.Key, Score = g.Sum(r => r.Score)});
            var winner = scores.OrderByDescending(s => s.Score).First().User;
            Winner = winner;
        }

        public void GenerateRandomBoard()
        {
            foreach (var round in Rounds)
            {
                round.Letters = BoardHelper.GenerateBoard();
            }
        }

        public void Resign(User resigner)
        {
            CurrentState = LetterHeadShared.DTO.Match.MatchState.Ended;
            Winner = Users.FirstOrDefault(u => u.Id != resigner.Id);
        }

        public void ClearMatch(User clearer)
        {
            if(ClearedUserIds.Contains(clearer.Id))
                return;

            ClearedUserIds.Add(clearer.Id);
        }
    }
}