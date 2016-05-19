using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hangfire;
using LetterHeadServer.Models;

namespace LetterHeadServer.Controllers
{
    public class BackendController : Controller
    {
        public string StartJobs()
        {
            RecurringJob.AddOrUpdate(() => DailyGame.CreateNewDailyGame(), Cron.Daily);
            return "Started Jobs";
        }

        public string NewDailyGame()
        {
            DailyGame.CreateNewDailyGame();
            return "New daily game created";
        }
    }
}