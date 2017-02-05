using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Facebook;
using Hangfire;
using LetterHeadServer.Classes;
using LetterHeadServer.Models;
using LetterHeadShared;
using MatchRound = LetterHeadShared.DTO.MatchRound;

namespace LetterHeadServer.Controllers
{
    public class BackendController : BaseLetterHeadController
    {
        private bool IsAuthenticated()
        {
            return HttpContext.Session["authenticated"] != null && (bool)HttpContext.Session["authenticated"] == true;
        }

        public ActionResult Index()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            ViewBag.UserCount = db.Users.Count();
            var yesterday = DateTime.Now.AddDays(-1);
            ViewBag.Last24 = db.Users.Where(u => u.SignupDate > yesterday).OrderByDescending(u => u.SignupDate).ToList();

            var last2ActiveCount = 0;
            var last7ActiveCount = 0;

            var users = db.Users.ToList();
            foreach (var user in users)
            {
                var lastRound = db.MatchRounds.Where(m => (m.CurrentState == MatchRound.RoundState.Active || m.CurrentState == MatchRound.RoundState.Ended) &&
                                                          m.User.Id == user.Id).OrderByDescending(o => o.StartedOn).FirstOrDefault();

                if (lastRound != null && lastRound.StartedOn.HasValue)
                {
                    if((DateTime.Now - lastRound.StartedOn.Value).TotalDays <= 2)
                    {
                        last2ActiveCount++;
                    }

                    if ((DateTime.Now - lastRound.StartedOn.Value).TotalDays <= 7)
                    {
                        last7ActiveCount++;
                    }
                }
            }

            ViewBag.last2ActiveCount = last2ActiveCount;
            ViewBag.last7ActiveCount = last7ActiveCount;
            ViewBag.newDaily = 0;

            foreach (CategoryManager.Type scoringType in Enum.GetValues(typeof (CategoryManager.Type)))
            {
                var curDaily = DailyGame.Current(scoringType, db);
                if (curDaily != null)
                {
                    ViewBag.newDaily += db.Matches.Count(m => m.DailyGame != null && m.DailyGame.Id == curDaily.Id);
                }
            }

            ViewBag.newSolo = db.Matches.Count(m => m.CreatedOn > yesterday && m.DailyGame == null && m.Users.Count == 1);
            ViewBag.newMatches = db.Matches.Count(m => m.CreatedOn > yesterday && m.DailyGame == null && m.Users.Count == 2);

            return View();
        }

        public ActionResult MatchStatistics()
        {
            var filterUserid = 2;

            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            var solo = db.Matches.Where(m => m.Users.Count == 1 && m.DailyGame == null && !m.Users.Any(u => u.Id == filterUserid)).ToList();
            var daily = db.Matches.Where(m => m.DailyGame != null && !m.Users.Any(u => u.Id == filterUserid)).ToList();
            var match = db.Matches.Where(m => m.Users.Count == 2 && !m.Users.Any(u => u.Id == filterUserid)).ToList();

            ViewBag.solo_total = solo.Count();
            ViewBag.daily_total = daily.Count();
            ViewBag.match_total = match.Count();


            ViewBag.solo_completed = solo.Count(m => m.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended && m.Resigner == null && m.CurrentRoundNumber == 8);
            ViewBag.daily_completed = daily.Count(m => m.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended && m.Resigner == null && m.CurrentRoundNumber == 8);
            ViewBag.match_completed = match.Count(m => m.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended && m.Resigner == null && m.CurrentRoundNumber == 8);

            ViewBag.solo_round1 = solo.Count(m => m.CurrentRoundNumber == 0);
            ViewBag.daily_round1 = daily.Count(m => m.CurrentRoundNumber == 0);
            ViewBag.match_round1 = match.Count(m => m.CurrentRoundNumber == 0);

            ViewBag.totalUsers = db.Users.Count();
            ViewBag.zeroMatchUsers = db.Users.Count(u => u.Matches.Count == 0);

            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult LoginPost(string password)
        {
            if (password == "l3tterHeAd000")
            {
                HttpContext.Session["authenticated"] = true;
                return RedirectToAction("Index");
            }

            return RedirectToAction("Login");
        }

        public string StartJobs()
        {
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");


            foreach (CategoryManager.Type scoringType in Enum.GetValues(typeof (CategoryManager.Type)))
            {
                RecurringJob.AddOrUpdate(() => DailyGame.CreateNewDailyGame(scoringType), Cron.Daily, easternZone);
            }

            RecurringJob.AddOrUpdate(() => new BackendController().AutoResignMatches(), Cron.Daily);
            RecurringJob.AddOrUpdate(() => new BackendController().ClearCompletedMatches(), Cron.Daily);
            
            return "Started Jobs";
        }

        public string AverageScore(int userId)
        {
            var user = db.Users.Find(userId);

            var matches = user.Matches.Where(m => m.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended);

            var Stat_GamesPlayedNoResigner = 0;
            var Stat_TotalScore = 0;

            foreach (var match in matches)
            {
                if (match.Resigner == null)
                {
                    // Check stats for games that didn't have a resigner
                    Stat_GamesPlayedNoResigner++;

                    var score = match.UserScore(user);

                    Stat_TotalScore += score;
                    Response.Write("Match " + match.Id + " - Score = " + score + "<br>");
                }
            }

            return "<p><b>Total Score = " + Stat_TotalScore + " Games Played = " + Stat_GamesPlayedNoResigner + " Average = " + (Stat_TotalScore/Stat_GamesPlayedNoResigner) +
                "<p>User stats reports " + user.Stat_TotalScore + " total score with " + user.Stat_GamesPlayedNoResigner + " games played (" + (user.Stat_TotalScore / user.Stat_GamesPlayedNoResigner) + " average)";
        }


        public string NewDailyGame(CategoryManager.Type scoringType = CategoryManager.Type.Normal)
        {
            DailyGame.CreateNewDailyGame(scoringType);
            return "New " + scoringType + " daily game created";
        }

        public ActionResult Matchmaking()
        {
            return Content("Matchmaking complete");
        }

        public string TestGame(int userId, int userId2)
        {
            var match = Match.New(db, new List<User>() { db.Users.Find(userId), db.Users.Find(userId2) }, CategoryManager.Type.Normal, 1);
            match.Initizile(db);

            db.SaveChanges();

            return "Match created!";
        }

        public string TestRtmMessage(int matchId)
        {
            var match = db.Matches.Find(matchId);

            match.Initizile(db);

            db.SaveChanges();

            return "Match created!";
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

        public void TestNotification(int userId, string title, string message)
        {
            SendNotification(userId, new NotificationDetails()
                                     {
                                         type = NotificationDetails.Type.Invite,
                                         title = title,
                                         content = message,
                                         tag = "invite123"
                                     });
        }

        public string AutoResignMatches()
        {
            var oldMatches = db.Matches.Where(m => m.CurrentState == LetterHeadShared.DTO.Match.MatchState.Running).ToList();
            foreach (var match in oldMatches)
            {
                if (!match.CurrentRound().ActivatedOn.HasValue)
                    continue;

                if ((DateTime.Now - match.CurrentRound().ActivatedOn.Value).TotalDays > 7)
                {
                    match.Resign(match.CurrentUserTurn);
                    Response?.Write("Resigned " + match.Id + "<br>");
                }
            }

            // Not started (but matched)
            var oldMatchTime = DateTime.Now - TimeSpan.FromDays(7);
            oldMatches = db.Matches.Where(m => m.CurrentState == LetterHeadShared.DTO.Match.MatchState.Pregame && m.CreatedOn < oldMatchTime).ToList();
            foreach (var match in oldMatches)
            {
                match.Resign(match.CurrentUserTurn);
                Response?.Write("Resigned " + match.Id + " (pregame)<br>");
            }


            db.SaveChanges();

            return "Done";
        }

        public string ClearCompletedMatches()
        {
            var oldMatches = db.Matches.Where(m => m.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended).ToList();
            foreach (var match in oldMatches)
            {
                if(!match.EndedOn.HasValue)
                    continue;
                if ((DateTime.Now - match.EndedOn.Value).TotalDays > 30)
                {
                    db.Matches.Remove(match);
                    Response?.Write("Deleted " + match.Id + "<br>");
                }
            }

            db.SaveChanges();

            return "Done";
        }

        public void SendNotification(int userId, NotificationDetails message)
        {
            var user = db.Users.Find(userId);
            if (user == null)
                return;

            user.NotificationBadgeCount++;
            db.SaveChanges();

            var sender = new NotificationSender();

            if (!string.IsNullOrEmpty(user.AndroidNotificationToken))
                sender.SendAndroid(user, message);

            if (!string.IsNullOrEmpty(user.IosNotificationToken))
                sender.SendIOS(user, message);
        }

        public void RefreshFacebookInfo(int userId)
        {
            var user = db.Users.Find(userId);
            if(user == null)
                return;

            if(string.IsNullOrEmpty(user.FacebookToken))
                return;

            var client = new FacebookClient(user.FacebookToken);
            dynamic info;

            try
            {
                info = client.Get("me?fields=id,name,friends,picture.type(large)", null);
            }
            catch (FacebookApiException)
            {
                return;
                throw;
            }

            if (info.error != null)
            {
                return;
            }

            if (user.AvatarUrl == user.FacebookPictureUrl)
            {
                user.AvatarUrl = info.picture.data.url;
            }

            var friends = info.friends.data;
            foreach (var friend in friends)
            {
                var fbId = (string) friend.id;
                var friendUser = db.Users.FirstOrDefault(u => u.FacebookId == fbId);
                if(friendUser == null)
                    continue;
                
                if(!user.Friends.Contains(friendUser))
                {
                    user.Friends.Add(friendUser);
                }
            }

            user.FacebookPictureUrl = info.picture.data.url;

            db.SaveChanges();
        }
    }
}