using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LetterHeadServer.Models;

namespace LetterHeadServer.Controllers
{
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

            //var match = Match.GetDailyMatch()
            return Json("ok");
        }
    }
}