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
                existingMatch.ChooseRandomStartingUser();
            }
            else
            {
                var match = Match.New(db, new List<User>() {currentUser}, 3);
                match.Letters = BoardHelper.GenerateBoard();
            }

            db.SaveChanges();

            return Okay();
        }

        public ActionResult List()
        {
            var matches = currentUser.Matches.Where(m => m.CurrentState != LetterHeadShared.DTO.Match.MatchState.WaitingForPlayers);
            return Json(matches.Select(m => m.DTO(true)));
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
            if(category == null)
            {
                return Error("No category selected");
            }

            var rounds = match.UserRounds(currentUser);
            if (rounds.Any(r => r.CategoryName == category.name))
            {
                return Error("That category has already been used");
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