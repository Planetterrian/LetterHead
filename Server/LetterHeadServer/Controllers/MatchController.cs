using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Hangfire;
using LetterHeadServer.Classes;
using LetterHeadServer.Models;
using LetterHeadShared;
using MyWebApplication;
using MatchRound = LetterHeadShared.DTO.MatchRound;

namespace LetterHeadServer.Controllers
{
    [AuthenticationFilter]
    public class MatchController : BaseLetterHeadController
    {
        private static object dailyGameLock = new object();
        private static object matchLock = new object();

        // GET: Match
        public ActionResult RequestDailyGameStart(CategoryManager.Type scoringType = CategoryManager.Type.Legacy)
        {
            var dailyGame = DailyGame.Current(scoringType);
            if (dailyGame == null)
            {
                return Error("No daily game available.");
            }

            lock (dailyGameLock)
            {
                var match = currentUser.GetMatch(db, dailyGame);
                if (match != null && match.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended)
                {
                    return Error("You already completed your daily game.");
                }

                if (!dailyGame.CanStart())
                {
                    return Error("The daily game is about to end. Try again soon.");
                }

                if (match == null)
                {
                    match = dailyGame.CreateMatchForUser(db, currentUser);
                }

                return Json(match.Id);
            }
        }

        public ActionResult RequestSoloGameStart(CategoryManager.Type scoringType = CategoryManager.Type.Legacy)
        {
            var rounds = (Environment.UserName == "Pete") ? 5 : 9;
            var match = Match.New(db, new List<User>() {currentUser}, scoringType, rounds);

            match.Initizile(db);

            db.SaveChanges();

            return Json(match.Id);
        }


        private class LeaderboardRowData
        {
            public int Score;
            public int Rank;
            public string Username;
        }

        public ActionResult IsSoloHighScore(int matchId, CategoryManager.Type scoringType = CategoryManager.Type.Legacy)
        {
            var match = Match.GetById(db, matchId);
            if (match == null)
            {
                return Error("Invalid Match");
            }

            if (!match.UserAuthorized(currentUser))
            {
                return Error("You can't access that match.");
            }

            if (currentUser.HighScoreMatchId == match.Id)
            {
                return Json("Y");
            }

            return Json("N");
        }

        public ActionResult DailyLeaderbaord(int dayOffset = 0, int maxResults = 25, CategoryManager.Type scoringType = CategoryManager.Type.Legacy)
        {
            var curDaily = DailyGame.Current(scoringType);
            var curDailyId = curDaily.Id;

            if (dayOffset > 0)
            {
                var date = curDaily.StartDate - TimeSpan.FromDays(dayOffset);
                var match = db.DailyGames.FirstOrDefault(d => DbFunctions.TruncateTime(d.StartDate) == DbFunctions.TruncateTime(date));
                if (match == null)
                {
                    return Error("No daily game found");
                }

                curDailyId = match.Id;
            }

            var dailyMatch = db.DailyGames.Find(curDailyId);

            var totalPlayers = db.Matches.Count(m => m.DailyGame != null && m.DailyGame.Id == curDailyId && m.ScoringType == scoringType);

            var top5 = db.Matches.Where(m => m.DailyGame != null && m.DailyGame.Id == curDailyId && m.ScoringType == scoringType).OrderByDescending(m => m.SingleScore).Take(maxResults).ToList();
            Match currentUserMatch = null;
            var currentUserRank = -1;

            for (int index = 0; index < top5.Count; index++)
            {
                var match = top5[index];
                if (match.Users[0].Id == currentUser.Id)
                {
                    currentUserMatch = match;
                    currentUserRank = index + 1;
                    break;
                }
            }

            if (currentUserMatch == null)
            {
                var allMatches = db.Matches.Where(m => m.DailyGame != null && m.DailyGame.Id == curDailyId && m.ScoringType == scoringType).OrderByDescending(m => m.SingleScore).ToList();
                for (int index = 0; index < allMatches.Count; index++)
                {
                    var match = allMatches[index];
                    if (match.Users[0].Id == currentUser.Id)
                    {
                        currentUserMatch = match;
                        currentUserRank = index + 1;
                        break;
                    }
                }
            }

            var ret = new List<LeaderboardRowData>();

            for (int index = 0; index < top5.Count; index++)
            {
                var match = top5[index];

                ret.Add(new LeaderboardRowData()
                        {
                            Rank = index + 1,
                            Username = match.Users[0].Username,
                            Score = match.SingleScore
                        });
            }

            if (currentUserRank > maxResults)
            {
                ret.Add(new LeaderboardRowData()
                {
                    Rank = currentUserRank,
                    Username = currentUserMatch.Users[0].Username,
                    Score = currentUserMatch.SingleScore,

                });
            }

            return Json(new
                        {
                            Scores = ret,
                            DateString = dailyMatch.StartDate.ToString("MM/dd/yyyy"),
                            TotalPlayers = totalPlayers,
                            MyRank = currentUserRank,
                        });
        }

        public ActionResult Random(CategoryManager.Type scoringType = CategoryManager.Type.Legacy)
        {
            lock (matchLock)
            {
                var curPending = currentUser.Matches.Count(m => m.CurrentState == LetterHeadShared.DTO.Match.MatchState.WaitingForPlayers && m.ScoringType == scoringType);
                if (curPending > 0)
                {
                    return Error("You are already searching for a random opponent.");
                }

                var existingMatch = db.Matches.FirstOrDefault(m => m.CurrentState == LetterHeadShared.DTO.Match.MatchState.WaitingForPlayers && m.Users.Count == 1 && m.ScoringType == scoringType);

                if (existingMatch != null)
                {
                    existingMatch.Users.Add(currentUser);
                    existingMatch.Initizile(db, false);
                }
                else
                {
                    var match = Match.New(db, new List<User>() {currentUser}, scoringType);
                }

                db.SaveChanges();

                return Okay();
            }
        }

        public ActionResult List()
        {
            var matches = currentUser.Matches.Where(m => m.CurrentState != LetterHeadShared.DTO.Match.MatchState.WaitingForPlayers);

            // Remove cleared
            matches = matches.Where(m => !m.ClearedUserIds.Contains(currentUser.Id)).ToList();

            var matchDTOs = new List<LetterHeadShared.DTO.Match>();
            foreach (var match in matches)
            {
                var dto = match.DTO(true);
                dto.LastTurnInfo = match.LastTurnString(currentUser);
                if (match.Users.Count > 1)
                    dto.UnreadChatMessageCount = ChatMessage.UnreadCount(db, currentUser, match.Users.First(m => m.Id != currentUser.Id).Id);
                matchDTOs.Add(dto);
            }
            
            // Legacy daily
            var dailyGame = DailyGame.Current(CategoryManager.Type.Legacy);
            var dailyMatch = currentUser.GetMatch(db, dailyGame);
            var canDoDaily = true;

            if (dailyMatch != null && dailyMatch.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended)
            {
                canDoDaily = false;
            }

            if (!dailyGame.CanStart())
            {
                canDoDaily = false;
            }


            var canDoDailyArray = new bool[Enum.GetValues(typeof(CategoryManager.Type)).Length];

            foreach (CategoryManager.Type scoringType in Enum.GetValues(typeof(CategoryManager.Type)))
            {
                dailyGame = DailyGame.Current(scoringType);

                if (dailyGame == null)
                {
                    canDoDailyArray[(int)scoringType] = false;
                    continue;
                }

                dailyMatch = currentUser.GetMatch(db, dailyGame);
                canDoDailyArray[(int)scoringType] = true;

                if (dailyMatch != null && dailyMatch.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended)
                {
                    canDoDailyArray[(int)scoringType] = false;
                }

                if (!dailyGame.CanStart())
                {
                    canDoDailyArray[(int)scoringType] = false;
                }
            }

            currentUser.NotificationBadgeCount = 0;
            db.SaveChanges();

            return Json(new { Matches = matchDTOs, Invites = currentUser.Invites.Select(i => i.DTO()), CanDoDaily = canDoDaily, CanDoDailyArray = canDoDailyArray } );
        }

        public ActionResult AcceptInvite(int inviteId)
        {
            lock (matchLock)
            {
                var invite = currentUser.Invites.FirstOrDefault(i => i.Id == inviteId);
                if (invite == null)
                {
                    return Error("Invalid Invite");
                }

                var users = new List<User>() {currentUser, invite.Inviter};
                users = users.OrderBy(u => Guid.NewGuid()).ToList();

                var match = Match.New(db, users, invite.ScoringType);
                match.Initizile(db);
                currentUser.Invites.Remove(invite);
                db.Entry(invite).State = EntityState.Deleted;

                db.SaveChanges();

                return Okay();
            }
        }

        public ActionResult DeclineInvite(int inviteId)
        {
            lock (matchLock)
            {
                var invite = currentUser.Invites.FirstOrDefault(i => i.Id == inviteId);
                if (invite == null)
                {
                    return Error("Invalid Invite");
                }

                currentUser.Invites.Remove(invite);
                db.Entry(invite).State = EntityState.Deleted;
                db.SaveChanges();

                return Okay();
            }
        }

        public ActionResult Invite(int userId, CategoryManager.Type scoringType = CategoryManager.Type.Legacy)
        {
            lock (matchLock)
            {
                var targetUser = db.Users.Find(userId);
                if (targetUser == null)
                {
                    return Error("Invalid User");
                }

                return DoInvite(targetUser, scoringType);
            }
        }


        public ActionResult InviteByUsername(string username, CategoryManager.Type scoringType = CategoryManager.Type.Legacy)
        {
            lock (matchLock)
            {
                if (string.IsNullOrEmpty(username))
                {
                    return Error("Please enter a username");
                }

                var targetUser = db.Users.FirstOrDefault(u => u.Username == username);
                if (targetUser == null)
                {
                    return Error("Invalid User");
                }

                return DoInvite(targetUser, scoringType);
            }
        }

        private ActionResult DoInvite(User targetUser, CategoryManager.Type scoringType)
        {
            if(targetUser.Id == currentUser.Id)
                return Error("You cannot invite yourself!");

            if (targetUser.Invites.Any(i => i.Inviter.Id == currentUser.Id))
            {
                return Error("You already sent an invite to that user.");
            }

            var invite = new Invite()
            {
                InviteSentOn = DateTime.Now,
                Inviter = currentUser,
                User = targetUser,
                ScoringType = scoringType
            };

            targetUser.Invites.Add(invite);
            db.SaveChanges();

            targetUser.SendNotification(new NotificationDetails()
            {
                content = "You have a Letter Head match invitation from " + currentUser.Username + ".",
                tag = "",
                title = "Leter Head Match Invite",
                type = NotificationDetails.Type.Invite
            });

            return Okay();
        }

        public ActionResult Resign(int matchId)
        {
            var match = Match.GetById(db, matchId);
            if (match == null)
            {
                return Error("Invalid Match");
            }

            if (!match.UserAuthorized(currentUser))
            {
                return Error("You can't access that match.");
            }

            match.Resign(currentUser);
            db.SaveChanges();

            return Okay();
        }

        public ActionResult Buzz(int matchId)
        {
            var match = Match.GetById(db, matchId);
            if (match == null)
            {
                return Error("Invalid Match");
            }

            if (!match.UserAuthorized(currentUser))
            {
                return Error("You can't access that match.");
            }

            if (match.CurrentUserTurn == currentUser)
            {
                return Error("It's your turn!");
            }

            var lastBuzz = match.Buzzes.Where(b => b.SourceUser.Id == currentUser.Id).OrderByDescending(b => b.date).FirstOrDefault();
            if (lastBuzz != null && (DateTime.Now - lastBuzz.date).TotalHours < 24)
            {
                return Error("Please wait before sending another poke.");
            }

            if ((DateTime.Now - match.CurrentRound().ActivatedOn.Value).TotalHours < 12)
            {
                return Error("This user played recently.  Please wait before poking.");
            }

            match.Buzzes.Add(new MatchBuzz()
                             {
                                 match = match,
                                 DestinationUser = match.CurrentUserTurn,
                                 SourceUser = currentUser,
                                 date = DateTime.Now
                             });

            db.SaveChanges();

            match.CurrentUserTurn.SendNotification(new NotificationDetails()
                                                   {
                                                       content = currentUser.Username + " poked you! It's your turn.",
                                                       title = "Letter Head - Poke",
                                                       tag = "buzz" + currentUser.Username,
                                                       type = NotificationDetails.Type.Buzz
                                                   });

            return Okay();
        }

        public ActionResult ClearAll()
        {
            var matches = currentUser.Matches.Where(m => m.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended).ToList();

            foreach (var match in matches)
            {
                match.ClearMatch(currentUser);
            }

            db.SaveChanges();

            return Okay();
        }

        public ActionResult Clear(int matchId)
        {
            var match = Match.GetById(db, matchId);
            if (match == null)
            {
                return Error("Invalid Match");
            }

            if (!match.UserAuthorized(currentUser))
            {
                return Error("You can't access that match.");
            }


            if (match.CurrentState != LetterHeadShared.DTO.Match.MatchState.Ended)
            {
                return Error("Match hassn't ended.");
            }


            match.ClearMatch(currentUser);
            db.SaveChanges();

            return Okay();
        }

        public ActionResult MatchInfo(int matchId)
        {
            var match = Match.GetById(db, matchId);
            if (match == null)
            {
                return Error("Invalid Match");
            }

            if (!match.UserAuthorized(currentUser))
            {
                return Error("You can't access that match.");
            }

            var dto = match.DTO();

            if(match.Users.Count > 1)
                dto.UnreadChatMessageCount = ChatMessage.UnreadCount(db, currentUser,
                    match.Users.First(m => m.Id != currentUser.Id).Id);

            return Json(dto);
        }
        

        public ActionResult SetCategory(int matchId, string categoryName)
        {
            lock (matchLock)
            {

                var match = Match.GetById(db, matchId);
                if (match == null)
                {
                    return Error("Invalid Match");
                }

                if (!match.UserAuthorized(currentUser))
                {
                    return Error("You can't access that match.");
                }

                if (match.CurrentUserTurn != currentUser)
                {
                    return Error("It's your opponents turn.");
                }

                if (match.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended)
                {
                    return Error("That game already ended.");
                }

                var round = match.CurrentRoundForUser(currentUser);
                if (round.CurrentState == LetterHeadShared.DTO.MatchRound.RoundState.Ended ||
                    round.CurrentState == LetterHeadShared.DTO.MatchRound.RoundState.NotStarted)
                {
                    return Okay();
                }

                var category = match.CategoryManager().GetCategory(categoryName);
                if (category == null)
                {
                    return Error("No category selected.");
                }

                if (category.alwaysActive)
                {
                    return Error("Category selected (Err 20)");
                }

                var rounds = match.UserRounds(currentUser).Where(r => r.Number != match.CurrentRoundNumber);
                if (rounds.Any(r => r.CategoryName == category.name))
                {
                    return Error("That category has already been used.");
                }


                round.CurrentState = MatchRound.RoundState.WaitingForCategory;

                round.SetCategory(db, category);

                return Okay();
            }
        }

        public void ManuallyEndRound(int matchId, int roundId)
        {
            var match = Match.GetById(db, matchId);

            var round = db.MatchRounds.Find(roundId);
            if (round.CurrentState == LetterHeadShared.DTO.MatchRound.RoundState.Active)
            {
                round.End();
                db.SaveChanges();
            }
        }

        public ActionResult UseStealTime(int matchId)
        {
            lock (matchLock)
            {
                var match = Match.GetById(db, matchId);
                if (match == null)
                {
                    return Error("Invalid Match");
                }

                if (!match.UserAuthorized(currentUser))
                {
                    return Error("You can't access that match.");
                }

                if (match.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended)
                {
                    return Error("That game already ended.");
                }

                var round = match.CurrentRoundForUser(currentUser);
                if (match.HasStealTimeBeenUsed(currentUser))
                {
                    return Error("Steal time already used.");
                }

                if (currentUser.PowerupCount(Powerup.Type.StealTime) < 1)
                {
                    return Error("No boost available.");
                }

                var nextRound = match.NextOpponentRoundNotStarted(currentUser);
                if (nextRound == null)
                {
                    return Error("This is the last round!");
                }

                if (nextRound.CurrentState != MatchRound.RoundState.NotStarted)
                {
                    return Error("Their round already started!");
                }

                round.StealTimeUsed = true;
                nextRound.StealTimeDelay = (float) (BoardHelper.rand.NextDouble()*10f) + 10f;
                currentUser.ConsumePowerup(Powerup.Type.StealTime);
                db.SaveChanges();

                return Okay();
            }
        }

        public ActionResult UseStealLetter(int matchId)
        {
            lock (matchLock)
            {
                var match = Match.GetById(db, matchId);
                if (match == null)
                {
                    return Error("Invalid Match");
                }

                if (!match.UserAuthorized(currentUser))
                {
                    return Error("You can't access that match.");
                }

                if (match.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended)
                {
                    return Error("That game already ended.");
                }

                var round = match.CurrentRoundForUser(currentUser);
                if (match.HasStealLetterBeenUsed(currentUser))
                {
                    return Error("Steal letter already used.");
                }

                if (currentUser.PowerupCount(Powerup.Type.StealLetter) < 1)
                {
                    return Error("No boost available.");
                }

                var nextRound = match.NextOpponentRoundNotStarted(currentUser);
                if (nextRound == null)
                {
                    return Error("This is the last round!");
                }

                if (nextRound.CurrentState != MatchRound.RoundState.NotStarted)
                {
                    return Error("Their round already started!");
                }

                round.StealLetterUsed = true;
                nextRound.StealLetterDelay = (float) (BoardHelper.rand.NextDouble()*20f) + 10f;
                currentUser.ConsumePowerup(Powerup.Type.StealLetter);
                db.SaveChanges();

                return Okay();
            }
        }
    }
}