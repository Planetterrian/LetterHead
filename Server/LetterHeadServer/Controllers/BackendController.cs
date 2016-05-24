using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Facebook;
using Hangfire;
using LetterHeadServer.Models;

namespace LetterHeadServer.Controllers
{
    public class BackendController : BaseLetterHeadController
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

        public ActionResult Matchmaking()
        {
            return Content("Matchmaking complete");
        }

        public string DeleteUserMatches(int userId)
        {
            var rounds = db.Users.Find(userId).Matches.ToList();

            foreach (var match in rounds)
            {
                db.Matches.Remove(match);
            }

            db.SaveChanges();

            return "Deleted " + rounds.Count + " matches for " + userId;
        }

        public void RefreshFacebookInfo(int userId)
        {
            var user = db.Users.Find(userId);
            if(user == null)
                return;

            if(string.IsNullOrEmpty(user.FacebookToken))
                return;

            var client = new FacebookClient(user.FacebookToken);
            dynamic info = client.Get("me?fields=id,name,picture.type(large)", null);

            if (info.error != null)
            {
                return;
            }

            if (user.AvatarUrl == user.FacebookPictureUrl)
            {
                user.AvatarUrl = info.picture.data.url;
            }

            user.FacebookPictureUrl = info.picture.data.url;

            db.SaveChanges();
        }
    }
}