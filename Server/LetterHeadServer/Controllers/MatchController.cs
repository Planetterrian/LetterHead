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

        public ActionResult OnStart(int matchId)
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
            if (round.CurrentState == MatchRound.RoundState.NotStarted)
            {
                round.CurrentState = LetterHeadShared.DTO.MatchRound.RoundState.Active;
                round.StartedOn = DateTime.Now;

                BackgroundJob.Schedule(() => new MatchController().ManuallyEndRound(matchId, round.Id), round.EndTime());
            }

            match.CurrentState = LetterHeadShared.DTO.Match.MatchState.Running;
            db.SaveChanges();

            return Json(new {TimeRemaining = (round.EndTime() - DateTime.Now).TotalSeconds});
        }

        public ActionResult SubmitRound(int matchId, string wordsEncoded, int uniqueLetters, string categoryName)
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
                return Error("No category slected");
            }

            var words = wordsEncoded.Split('#').ToList();
            round.CalculateScore(words, uniqueLetters, category);
            round.End();

            db.SaveChanges();

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