using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hangfire;
using LetterHeadServer.Classes;
using LetterHeadServer.Models;
using MyWebApplication;
using MatchRound = LetterHeadShared.DTO.MatchRound;

namespace LetterHeadServer.Controllers
{
    [AuthenticationFilter]
    public class MatchController : BaseLetterHeadController
    {
        // GET: Match
        public ActionResult RequestDailyGameStart()
        {
            var dailyGame = DailyGame.Current();
            if (dailyGame == null)
            {
                return Error("No daily game available");
            }

            var match = currentUser.GetMatch(db, dailyGame);
            if (match != null && match.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended)
            {
                return Error("You have already completed your daily game");
            }

            if (match == null)
            {
                match = dailyGame.CreateMatchForUser(db, currentUser);
            }

            return Json(match.Id);
        }

        public ActionResult Random()
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
                InitilizeMatch(existingMatch);
            }
            else
            {
                var match = Match.New(db, new List<User>() {currentUser}, 10);
            }

            db.SaveChanges();

            return Okay();
        }

        private void InitilizeMatch(Match match)
        {
            match.CurrentState = LetterHeadShared.DTO.Match.MatchState.Pregame;
            match.AddRounds(db);
            db.SaveChanges();

            match.GenerateRandomBoard();
            match.RandomizeUsers();
            db.SaveChanges();

            match.OnNewTurn();
        }

        public ActionResult List()
        {
            var matches = currentUser.Matches.Where(m => m.CurrentState != LetterHeadShared.DTO.Match.MatchState.WaitingForPlayers);

            // Remove cleared
            matches = matches.Where(m => !m.ClearedUserIds.Contains(currentUser.Id)).ToList();
            return Json(new { Matches = matches.Select(m => m.DTO(true)), Invites = currentUser.Invites.Select(i => i.DTO()) } );
        }

        public ActionResult AcceptInvite(int inviteId)
        {
            var invite = currentUser.Invites.FirstOrDefault(i => i.Id == inviteId);
            if (invite == null)
            {
                return Error("Invalid Invite");
            }

            var users = new List<User>() { currentUser, invite.Inviter };
            users = users.OrderBy(u => Guid.NewGuid()).ToList();

            var match = Match.New(db, users, 10);
            InitilizeMatch(match);
            currentUser.Invites.Remove(invite);
            db.Entry(invite).State = EntityState.Deleted;

            db.SaveChanges();

            return Okay();
        }

        public ActionResult DeclineInvite(int inviteId)
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

        public ActionResult Invite(int userId)
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
                return Error("Please wait before buzzing again");
            }

            if ((DateTime.Now - match.CurrentRound().ActivatedOn.Value).TotalHours < 12)
            {
                return Error("Please wait before buzzing");
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
                                                       content = currentUser.Username + " buzzed you! It's your turn.",
                                                       title = "LetterHead - Buzz",
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
        

        public ActionResult SetCategory(int matchId, string categoryName, bool endMatch)
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
                return Error("Invalid round Err 19");
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

            if (endMatch)
            {
                round.CurrentState = MatchRound.RoundState.WaitingForCategory;
            }

            round.SetCategory(db, category);

            return Okay();
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
    }
}