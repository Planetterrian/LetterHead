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
        public ActionResult RequestDailyGameStart()
        {
            var dailyGame = DailyGame.Current();
            if (dailyGame == null)
            {
                return Error("No daily game available");
            }

            lock (dailyGameLock)
            {
                var match = currentUser.GetMatch(db, dailyGame);
                if (match != null && match.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended)
                {
                    return Error("You have already completed your daily game");
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

        public ActionResult RequestSoloGameStart()
        {
            var rounds = (Environment.UserName == "Pete") ? 1 : 9;
            var match = Match.New(db, new List<User>() {currentUser}, rounds);

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

        public ActionResult IsSoloHighScore(int matchId)
        {
            var match = Match.GetById(db, matchId);
            if (match == null)
            {
                return Error("Invalid Match");
            }

            if (!match.UserAuthorized(currentUser))
            {
                return Error("You can't access that match");
            }

            var highRound = db.Matches.Where(m => m.Users.Any(u => u.Id == currentUser.Id) && m.DailyGame == null && m.Users.Count == 1).OrderByDescending(m => m.SingleScore).FirstOrDefault()?.Id;
            if (!highRound.HasValue || highRound == matchId)
                return Json("Y");

            return Json("N");
        }

        public ActionResult DailyLeaderbaord()
        {
            var curDailyId = DailyGame.Current().Id;

            var top5 = db.Matches.Where(m => m.DailyGame != null && m.DailyGame.Id == curDailyId).OrderByDescending(m => m.SingleScore).Take(5).ToList();
            Match currentUserMatch = null;
            var currentUserRank = 0;

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
                var allMatches = db.Matches.Where(m => m.DailyGame != null && m.DailyGame.Id == DailyGame.Current().Id).OrderByDescending(m => m.SingleScore).ToList();
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

            if (currentUserRank > 5)
            {
                ret.Add(new LeaderboardRowData()
                {
                    Rank = currentUserRank,
                    Username = currentUserMatch.Users[0].Username,
                    Score = currentUserMatch.SingleScore
                });
            }

            return Json(ret);
        }

        public ActionResult Random()
        {
            lock (matchLock)
            {
                var curPending = currentUser.Matches.Count(m => m.CurrentState == LetterHeadShared.DTO.Match.MatchState.WaitingForPlayers);
                if (curPending > 0)
                {
                    return Error("You are already searching for a random opponent.");
                }

                var existingMatch = db.Matches.FirstOrDefault(m => m.CurrentState == LetterHeadShared.DTO.Match.MatchState.WaitingForPlayers && m.Users.Count == 1);

                if (existingMatch != null)
                {
                    existingMatch.Users.Add(currentUser);
                    existingMatch.Initizile(db);
                }
                else
                {
                    var match = Match.New(db, new List<User>() {currentUser});
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
                matchDTOs.Add(dto);
            }

            return Json(new { Matches = matchDTOs, Invites = currentUser.Invites.Select(i => i.DTO()) } );
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

                var match = Match.New(db, users);
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

        public ActionResult Invite(int userId)
        {
            lock (matchLock)
            {
                var targetUser = db.Users.Find(userId);
                if (targetUser == null)
                {
                    return Error("Invalid User");
                }

                if (targetUser.Invites.Any(i => i.Inviter.Id == currentUser.Id))
                {
                    return Error("You have already sent an invite to that user");
                }

                var invite = new Invite()
                             {
                                 InviteSentOn = DateTime.Now,
                                 Inviter = currentUser,
                                 User = targetUser
                             };

                targetUser.Invites.Add(invite);
                db.SaveChanges();

                targetUser.SendNotification(new NotificationDetails()
                                            {
                                                content = "You have a LetterHead match invitation from " + currentUser.Username,
                                                tag = "",
                                                title = "LeterHead Match Invite",
                                                type = NotificationDetails.Type.Invite
                                            });

                return Okay();
            }
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
                return Error("You can't access that match");
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
                return Error("You can't access that match");
            }

            if (match.CurrentUserTurn == currentUser)
            {
                return Error("It's your turn!");
            }

            var lastBuzz = match.Buzzes.Where(b => b.SourceUser.Id == currentUser.Id).OrderByDescending(b => b.date).FirstOrDefault();
            if (lastBuzz != null && (DateTime.Now - lastBuzz.date).TotalHours < 12)
            {
                return Error("Please wait before sending another poke");
            }

            if ((DateTime.Now - match.CurrentRound().ActivatedOn.Value).TotalHours < 12)
            {
                return Error("Please wait before poking");
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
                                                       title = "LetterHead - Poke",
                                                       tag = "buzz" + currentUser.Username,
                                                       type = NotificationDetails.Type.Buzz
                                                   });

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
                return Error("You can't access that match");
            }


            if (match.CurrentState != LetterHeadShared.DTO.Match.MatchState.Ended)
            {
                return Error("Match isn't ended");
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
                return Error("You can't access that match");
            }

            var dto = match.DTO();

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
                    return Error("You can't access that match");
                }

                if (match.CurrentUserTurn != currentUser)
                {
                    return Error("It's your opponents turn");
                }

                if (match.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended)
                {
                    return Error("That game has already ended");
                }

                var round = match.CurrentRoundForUser(currentUser);
                if (round.CurrentState == LetterHeadShared.DTO.MatchRound.RoundState.Ended ||
                    round.CurrentState == LetterHeadShared.DTO.MatchRound.RoundState.NotStarted)
                {
                    return Okay();
                }

                var category = Startup.CategoryManager.GetCategory(categoryName);
                if (category == null)
                {
                    return Error("No category selected");
                }

                if (category.alwaysActive)
                {
                    return Error("Category selected (Err 20)");
                }

                var rounds = match.UserRounds(currentUser).Where(r => r.Number != match.CurrentRoundNumber);
                if (rounds.Any(r => r.CategoryName == category.name))
                {
                    return Error("That category has already been used");
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
                    return Error("You can't access that match");
                }

                if (match.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended)
                {
                    return Error("That game has already ended");
                }

                var round = match.CurrentRoundForUser(currentUser);
                if (match.HasStealTimeBeenUsed(currentUser))
                {
                    return Error("Steal time already used");
                }

                if (currentUser.PowerupCount(Powerup.Type.StealTime) < 1)
                {
                    return Error("No boost available");
                }

                var nextRound = match.NextOpponentRoundNotStarted(currentUser);
                if (nextRound == null)
                {
                    return Error("This is the last round!");
                }

                if (nextRound.CurrentState != MatchRound.RoundState.NotStarted)
                {
                    return Error("Their round has already started!");
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
                    return Error("You can't access that match");
                }

                if (match.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended)
                {
                    return Error("That game has already ended");
                }

                var round = match.CurrentRoundForUser(currentUser);
                if (match.HasStealLetterBeenUsed(currentUser))
                {
                    return Error("Steal letter already used");
                }

                if (currentUser.PowerupCount(Powerup.Type.StealLetter) < 1)
                {
                    return Error("No boost available");
                }

                var nextRound = match.NextOpponentRoundNotStarted(currentUser);
                if (nextRound == null)
                {
                    return Error("This is the last round!");
                }

                if (nextRound.CurrentState != MatchRound.RoundState.NotStarted)
                {
                    return Error("Their round has already started!");
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