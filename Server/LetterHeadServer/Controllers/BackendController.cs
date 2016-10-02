using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Facebook;
using Hangfire;
using LetterHeadServer.Classes;
using LetterHeadServer.Models;

namespace LetterHeadServer.Controllers
{
    public class BackendController : BaseLetterHeadController
    {
        public string StartJobs()
        {
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            RecurringJob.AddOrUpdate(() => DailyGame.CreateNewDailyGame(), Cron.Daily, easternZone);
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


        public string NewDailyGame()
        {
            DailyGame.CreateNewDailyGame();
            return "New daily game created";
        }

        public ActionResult Matchmaking()
        {
            return Content("Matchmaking complete");
        }

        public string TestGame(int userId, int userId2)
        {
            var match = Match.New(db, new List<User>() { db.Users.Find(userId), db.Users.Find(userId2) }, 1);
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