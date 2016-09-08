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
                if (ClearedUserIds == null)
                    return "";

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


        public List<int> TurnOrderUserIds { get; set; }
        public string TurnOrderUserIdJoined
        {
            get
            {
                if (TurnOrderUserIds == null)
                    return "";

                return string.Join(",", TurnOrderUserIds);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    TurnOrderUserIds = new List<int>();
                else
                    TurnOrderUserIds = value.Split(',').Select(int.Parse).ToList();
            }
        }

        [Index]
        public virtual User Winner { get; set; }

        public bool IsTie { get; set; }


        public virtual User Resigner { get; set; }

        [Index]
        public LetterHeadShared.DTO.Match.MatchState CurrentState { get; set; }

        [Index]
        public virtual DailyGame DailyGame { get; set; }

        public virtual User CurrentUserTurn { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? EndedOn { get; set; }
        public int RoundTimeSeconds { get; set; }
        public int SingleScore { get; set; }

        public int MaxRounds { get; set; }

        public virtual ICollection<MatchRound> Rounds { get; set; }
        public virtual ICollection<MatchBuzz> Buzzes { get; set; }

        public int CurrentRoundNumber { get; set; }

        public Match()
        {
            TurnOrderUserIds = new List<int>();
        }

        public static Match New(ApplicationDbContext db, List<User> users, int roundCount = 9)
        {
            var roundTime = (Environment.UserName == "Pete") ? 20 : 120;
            
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
            {
                dto.Rounds = null;
            }

            return dto;
        }

        public string LastTurnString(User forUser)
        {
            var lastRound = PreviousRound();
            if (lastRound == null)
                return "";

            if (lastRound.ShieldUsed)
            {
                if (lastRound.StealTimeDelay > 0)
                {
                    if (lastRound.User == forUser)
                        return "You blocked your opponent's steal time!";
                    else
                        return "Your opponent blocked your steal!";
                }
                else if (lastRound.StealLetterDelay > 0)
                {
                    if (lastRound.User == forUser)
                        return "You blocked your opponent's steal letter!";
                    else
                        return "Your opponent blocked your steal!";
                }
            }
            else if(lastRound.StealSuccess)
            {
                if (lastRound.StealTimeDelay > 0)
                {
                    if (lastRound.User != forUser)
                        return "You stole time from your opponent!";
                }
                else if (lastRound.StealLetterDelay > 0)
                {
                    if (lastRound.User != forUser)
                        return "You stole a letter from your opponent!";
                }
            }

            var currentRound = CurrentRound();

            if (currentRound.StealTimeDelay > 0)
            {
                if (currentRound.User != forUser)
                {
                    return "You sent a timer sabotage!";
                }
            }
            else if (currentRound.StealLetterDelay > 0)
            {
                if (currentRound.User != forUser)
                    return "You sent a letter sabotage!";
            }

            return "";
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
            var currentUserIndex = TurnOrderUserIds.IndexOf(CurrentUserTurn.Id);
            currentUserIndex++;
            if (currentUserIndex >= Users.Count)
            {
                EndRound();
            }
            else
            {
                CurrentRoundForUser(CurrentUserTurn).CurrentState = LetterHeadShared.DTO.MatchRound.RoundState.Ended;
                CurrentUserTurn = Users.First(u => u.Id == TurnOrderUserIds[currentUserIndex]);
                OnNewTurn();
            }
        }

        public void OnNewTurn()
        {
            var round = CurrentRound();
            round.ActivatedOn = DateTime.Now;

            if (Users.Count == 1)
                return;

            CurrentUserTurn.SendNotification(new NotificationDetails()
            {
                content = "It's your turn in your match against " + Users.First(u => u.Id != CurrentUserTurn.Id).Username + ".",
                tag = "match" + Id,
                title = "LetterHead - Your turn",
                type = NotificationDetails.Type.YourTurn
            });
        }

        public MatchRound CurrentRound()
        {
            return Rounds.FirstOrDefault(r => r.User.Id == CurrentUserTurn.Id && r.Number == CurrentRoundNumber);
        }

        private void EndRound()
        {
            if (CurrentRoundNumber == MaxRounds - 1)
            {
                EndMatch();
                return;
            }

            CurrentUserTurn = Users.First(u => u.Id == TurnOrderUserIds[0]);
            CurrentRoundNumber++;
            OnNewTurn();
        }

        public List<MatchRound> UserRounds(User user)
        {
            return Rounds.Where(r => r.User.Id == user.Id).ToList();
        }

        public void RandomizeUsers()
        {
            TurnOrderUserIds.AddRange(Users.Select(u => u.Id));
            TurnOrderUserIds = TurnOrderUserIds.OrderBy(u => Guid.NewGuid()).ToList();
            CurrentUserTurn = Users.First(u => u.Id == TurnOrderUserIds[0]);
        }

        public List<int> GetScores()
        {
            var scores = new List<int>();

            for (int index = 0; index < Users.Count; index++)
            {
                var user = Users[index];
                var score = UserScore(user);
                scores.Add(score);
            }

            return scores;
        }

        public int UserScore(User user)
        {
            var score = Rounds.Where(r => r.User.Id == user.Id).Sum(r => r.Score) + MatchBonus(user);
            return score;
        }


        public int MatchBonus(User user)
        {
            var bigWord = Startup.CategoryManager.GetCategory("Big Word Bonus");
            var rounds = Rounds.Where(r => r.User.Id == user.Id);

            var bigWordBonus = rounds.Sum(round => bigWord.GetScore(round.Words, 0, new List<int>()));
            var upperBonus = Startup.CategoryManager.GetCategory("Upper Bonus").GetScore(null, 0, CategoryScores(user));

            return bigWordBonus + upperBonus;
        }


        public void EndMatch()
        {
            DetermineWinner();
            OnMatchFinished();
        }

        private void UpdatePlayerStats()
        {
            foreach (var user in Users)
            {

                if (Users.Count > 1)
                {
                    // Multiplayer game

                    if (Winner == user)
                        user.Stat_GamesWon++;
                    else if (IsTie)
                        user.Stat_GamesTied++;
                    else
                        user.Stat_GamesLost++;
                }

                if (Resigner == null)
                {
                    // Check stats for games that didn't have a resigner
                    user.Stat_GamesPlayedNoResigner++;

                    var score = UserScore(user);
                    if (score > user.Stat_BestScore)
                    {
                        user.Stat_BestScore = score;
                        user.HighScoreMatchId = Id;
                    }

                    user.Stat_TotalScore += score;
                }
            }
        }

        private void DetermineWinner()
        {
            if (Users.Count == 1)
            {
                SingleScore = GetScores()[0];
                Winner = Users[0];
                return;
            }

            var scores = Rounds.GroupBy(r => r.User).Select(g => new {User = g.Key, Score = g.Sum(r => r.Score) + MatchBonus(g.Key)}).ToList();

            var firstScore = scores[0].Score;
            var isTie = Users.Count > 1 && scores.All(scoreInfo => scoreInfo.Score == firstScore);

            if (isTie)
            {
                IsTie = true;
            }
            else
            {
                var winner = scores.OrderByDescending(s => s.Score).First().User;
                Winner = winner;
            }
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
            Winner = Users.FirstOrDefault(u => u.Id != resigner.Id);
            Resigner = resigner;
            OnMatchFinished();
        }

        private void OnMatchFinished()
        {
            CurrentState = LetterHeadShared.DTO.Match.MatchState.Ended;
            EndedOn = DateTime.Now;
            UpdatePlayerStats();
        }

        public void ClearMatch(User clearer)
        {
            if(ClearedUserIds.Contains(clearer.Id))
                return;

            ClearedUserIds.Add(clearer.Id);
        }

        public List<int> CategoryScores(User user)
        {
            var scores = new int[Startup.CategoryManager.Categories.Count];

            var rounds = Rounds.Where(m => m.User.Id == user.Id && m.Number <= CurrentRoundNumber);
            foreach (var round in rounds)
            {
                if (string.IsNullOrEmpty(round.CategoryName))
                    continue;

                var categoryIndex = Startup.CategoryManager.GetCategoryIndex(round.CategoryName);
                if(categoryIndex != -1)
                    scores[categoryIndex] = round.Score;
            }

            return scores.ToList();
        }

        public void Initizile(ApplicationDbContext db)
        {
            CurrentState = LetterHeadShared.DTO.Match.MatchState.Pregame;
            AddRounds(db);
            db.SaveChanges();

            GenerateRandomBoard();
            RandomizeUsers();
            db.SaveChanges();

            OnNewTurn();
        }


        public bool HasShieldBeenUsed(User user)
        {
            return Rounds.Any(r => r.ShieldUsed && r.User.Id == user.Id);
        }

        public bool HasDoOverBeenUsed(User user)
        {
            return Rounds.Any(r => r.DoOverUsed && r.User.Id == user.Id);
        }

        public bool HasStealTimeBeenUsed(User user)
        {
            return Rounds.Any(r => r.StealTimeUsed && r.User.Id == user.Id);
        }

        public bool HasStealLetterBeenUsed(User user)
        {
            return Rounds.Any(r => r.StealLetterUsed && r.User.Id == user.Id);
        }

        public MatchRound NextRound()
        {
            var roundNumber = CurrentRoundNumber;

            var currentUserIndex = TurnOrderUserIds.IndexOf(CurrentUserTurn.Id);
            currentUserIndex++;
            if (currentUserIndex >= Users.Count)
            {
                currentUserIndex = 0;
                roundNumber++;
            }

            if (roundNumber > MaxRounds)
            {
                return null;
            }

            return Rounds.First(r => r.Number == roundNumber && r.User.Id == Users[currentUserIndex].Id);
        }

        public MatchRound PreviousRound()
        {
            var roundNumber = CurrentRoundNumber;
            if (TurnOrderUserIds.Count == 0)
                return null;

            var currentUserIndex = TurnOrderUserIds.IndexOf(CurrentUserTurn.Id);
            currentUserIndex--;
            if (currentUserIndex < 0)
            {
                currentUserIndex = Users.Count - 1;
                roundNumber--;
            }

            if (roundNumber < 0)
            {
                return null;
            }

            return Rounds.FirstOrDefault(r => r.Number == roundNumber && r.User.Id == TurnOrderUserIds[currentUserIndex]);
        }

        public MatchRound GetRoundByIndex(int index)
        {
            var userIndexOffset = index%Users.Count;
            var roundNumber = index/Users.Count;

            if (roundNumber >= MaxRounds)
                return null;

            return Rounds.FirstOrDefault(r => r.Number == roundNumber && r.User.Id == TurnOrderUserIds[userIndexOffset]);
        }

        private int RoundIndex(MatchRound round)
        {
            var roundNumber = round.Number;
            var indexOffset = TurnOrderUserIds.IndexOf(round.User.Id);

            return roundNumber + indexOffset;
        }

        // Returns the next round that is an opponent's (or the current one if it's the opponent's turn)
        public MatchRound NextOpponentRoundNotStarted(User fromUser)
        {
            if (CurrentUserTurn != fromUser)
            {
                // It's currently the fromUser's turn
                if (CurrentRound().CurrentState == LetterHeadShared.DTO.MatchRound.RoundState.NotStarted)
                    return CurrentRound();

                var nextRound = GetRoundByIndex(RoundIndex(CurrentRound()) + 2);
                if (nextRound != null)
                    return nextRound;
            }

            if (NextRound() == null)
                return null;

            if (NextRound().CurrentState == LetterHeadShared.DTO.MatchRound.RoundState.NotStarted)
                return NextRound();

            var nextRound2 = GetRoundByIndex(RoundIndex(NextRound()) + 2);
            return nextRound2;
        }

        public void SendRtmMessage(RealTimeMatch.Message message)
        {
            var matchRtm = RealTimeMatchManager.GetMatch(Id);
            matchRtm.AddMessage(message);
        }
    }
}