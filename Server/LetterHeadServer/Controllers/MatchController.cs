using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LetterHeadServer.Classes;
using LetterHeadServer.Models;

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
            if (match != null && match.CurrentState == Match.State.Ended)
            {
                return Error("You have already completed your daily game");
            }

            if (match == null)
            {
                match = dailyGame.CreateMatchForUser(currentUser);
                db.Matches.Add(match);
                db.SaveChanges();
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

            return Json(match.DTO());
        }
    }
}