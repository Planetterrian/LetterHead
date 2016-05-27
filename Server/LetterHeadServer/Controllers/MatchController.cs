using System;
using System.Collections.Generic;
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
                existingMatch.CurrentState = LetterHeadShared.DTO.Match.MatchState.Pregame;
                existingMatch.AddRounds(db);
                db.SaveChanges();

                existingMatch.GenerateRandomBoard();
                existingMatch.RandomizeUsers();
            }
            else
            {
                var match = Match.New(db, new List<User>() {currentUser}, 10);
            }

            db.SaveChanges();

            return Okay();
        }

        public ActionResult List()
        {
            var matches = currentUser.Matches.Where(m => m.CurrentState != LetterHeadShared.DTO.Match.MatchState.WaitingForPlayers);

            // Remove cleared
            matches = matches.Where(m => !m.ClearedUserIds.Contains(currentUser.Id)).ToList();
            return Json(matches.Select(m => m.DTO(true)));
        }

        public ActionResult Invite(int userId)
        {
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