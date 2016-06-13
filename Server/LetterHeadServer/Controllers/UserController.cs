using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Facebook;
using Hangfire;
using LetterHeadServer.Classes;
using LetterHeadServer.Models;
using LetterHeadShared;
using MyWebApplication;
using Match = LetterHeadShared.DTO.Match;

namespace LetterHeadServer.Controllers
{
    public class UserController : BaseLetterHeadController
    {
        public ActionResult RegisterEmailWeb()
        {
            return View(new UserRegistrationModel());
        }

        public ActionResult RegisterEmail([Bind(Exclude = "Id")] UserRegistrationModel model)
        {
            if (ModelState.IsValid)
            {
                if (db.Users.Count(u => u.Email == model.Email) > 0)
                {
                    return Error("Email address already in use");
                }

                UserManager.CreateUser(db, model.Email, model.Password);
                var user = UserManager.LoginUserWithEmail(db, model);
                user.GenerateNewSessionId();
                db.SaveChanges();


                return Json(user.SessionId);
            }
            else
            {
                var err = ModelState.Select(x => x.Value.Errors).FirstOrDefault(y => y.Count > 0);
                return Error(err[0].ErrorMessage);
            }
        }

        [AuthenticationFilter()]
        public ActionResult UseFacebookImage()
        {
            if (!string.IsNullOrEmpty(currentUser.FacebookPictureUrl))
            {
                currentUser.AvatarUrl = currentUser.FacebookPictureUrl;
                db.SaveChanges();
            }

            return Okay();
        }

        public ActionResult FacebookLogin(string token)
        {
            var client = new FacebookClient(token);
            dynamic info = client.Get("me?fields=id,name,picture.type(large)", null);

            if (info.error != null)
            {
                return Error("Invalid token");
            }

            var facebookUser = new FacebookUserInfo()
            {
                Id = info.id,
                Token = token,
                PictureUrl = info.picture.data.url,
                Name = info.name,
            };

            var user = UserManager.LoginUserFromFacebook(db, facebookUser);
            user.GenerateNewSessionId();
            db.SaveChanges();

            return Json(user.SessionId, JsonRequestBehavior.AllowGet);
        }

        [AuthenticationFilter()]
        public ActionResult RegisterDetails(string username, string avatar)
        {
            ActionResult actionResult;

            if (UsernameIsInvalid(username, out actionResult))
                return actionResult;

            currentUser.Username = username;
            currentUser.AvatarUrl = "sprite:" + avatar;
            db.SaveChanges();
            return Okay();
        }

        [AuthenticationFilter()]
        public ActionResult AndroidNotificationToken(string token)
        {
            currentUser.AndroidNotificationToken = token;
            db.SaveChanges();
            return Okay();
        }

        [AuthenticationFilter()]
        public ActionResult IosNotificationToken(string token)
        {
            currentUser.IosNotificationToken = token;
            db.SaveChanges();
            return Okay();
        }


        [AuthenticationFilter()]
        public ActionResult IapPurchase(string productId, string receipt)
        {
            switch (productId)
            {
                case "com.we3workshop.letterhead.doover_small":
                    currentUser.AddPowerup(Powerup.Type.DoOver, 5);
                    break;
                case "com.we3workshop.letterhead.doover_large":
                    currentUser.AddPowerup(Powerup.Type.DoOver, 25);
                    break;
                case "com.we3workshop.letterhead.shield_small":
                    currentUser.AddPowerup(Powerup.Type.Shield, 5);
                    break;
                case "com.we3workshop.letterhead.shield_large":
                    currentUser.AddPowerup(Powerup.Type.Shield, 25);
                    break;
                case "com.we3workshop.letterhead.stealtime_small":
                    currentUser.AddPowerup(Powerup.Type.StealTime, 5);
                    break;
                case "com.we3workshop.letterhead.stealtime_large":
                    currentUser.AddPowerup(Powerup.Type.StealTime, 25);
                    break;
                case "com.we3workshop.letterhead.stealletter_small":
                    currentUser.AddPowerup(Powerup.Type.StealLetter, 5);
                    break;
                case "com.we3workshop.letterhead.stealletter_large":
                    currentUser.AddPowerup(Powerup.Type.StealLetter, 25);
                    break;
                case "com.we3workshop.letterhead.boosterpack_small":
                    currentUser.AddPowerup(Powerup.Type.DoOver, 5);
                    currentUser.AddPowerup(Powerup.Type.Shield, 5);
                    currentUser.AddPowerup(Powerup.Type.StealTime, 5);
                    currentUser.AddPowerup(Powerup.Type.StealLetter, 5);
                    break;
                case "com.we3workshop.letterhead.boosterpack_large":
                    currentUser.AddPowerup(Powerup.Type.DoOver, 15);
                    currentUser.AddPowerup(Powerup.Type.Shield, 15);
                    currentUser.AddPowerup(Powerup.Type.StealTime, 15);
                    currentUser.AddPowerup(Powerup.Type.StealLetter, 15);
                    break;
            }

            db.SaveChanges();
            return Okay();
        }


        private bool UsernameIsInvalid(string username, out ActionResult actionResult)
        {
            if (username.Length < 3 || username.Length > 24)
            {
                actionResult = Error("Username must be between 3 and 24 characters");
                return true;
            }

            if (currentUser != null)
            {
                if (db.Users.Count(u => u.Username == username && u.Id != currentUser.Id) > 0)
                {
                    actionResult = Error("Username already taken");
                    return true;
                }
            }
            else
            {
                if (db.Users.Count(u => u.Username == username) > 0)
                {
                    actionResult = Error("Username already taken");
                    return true;
                }
            }

            if (username.Contains("<"))
            {
                actionResult = Error("Username contains invalid characters");
                return true;
            }

            var badWords = Startup.BadWords;
            if (badWords.Any(username.Contains))
            {
                actionResult = Error("Username contains inappropriate words");
                return true;
            }

            actionResult = null;
            return false;
        }

        [AuthenticationFilter()]
        public ActionResult CanDoDailyPowerup()
        {
            return Json(currentUser.CanDoDailyPowerup());
        }

        [AuthenticationFilter()]
        public ActionResult DailyPowerup(int type)
        {
            if (!currentUser.CanDoDailyPowerup())
                return Error("Can't do daily powerup!");

            var powerupType = (Powerup.Type) type;
            currentUser.AddPowerup(powerupType, 1);

            var timeUtc = DateTime.UtcNow;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);

            currentUser.LastFreePowerup = easternTime;
            db.SaveChanges();

            return Okay();
        }

        [AuthenticationFilter()]
        public ActionResult SetAvatar(string sprite)
        {
            currentUser.AvatarUrl = "sprite:" + sprite;
            db.SaveChanges();
            return Okay();
        }

        [AuthenticationFilter()]
        public ActionResult SetUsername(string username)
        {
            ActionResult actionResult;
            if (UsernameIsInvalid(username, out actionResult))
                return actionResult;

            currentUser.Username = username;
            db.SaveChanges();
            return Okay();
        }

        public ActionResult Stats(int userId)
        {
            var user = db.Users.Find(userId);
            if (user == null)
                return Error("Unable to locate user");

            return Json(user.Stats(db));
        }

        [AuthenticationFilter()]
        public ActionResult NextAvailableMatch(int currentMatchId)
        {
            var match = currentUser.Matches.Where(m => m.CurrentState != Match.MatchState.Ended && m.CurrentUserTurn.Id == currentUser.Id && m.Id > currentMatchId).OrderBy(m => m.Id).FirstOrDefault();

            if(match == null)
                match = currentUser.Matches.Where(m => m.CurrentState != Match.MatchState.Ended && m.CurrentUserTurn.Id == currentUser.Id && m.Id != currentMatchId).OrderBy(m => m.Id).FirstOrDefault();

            return Json(match?.DTO());
        }

        public ActionResult LoginEmail([Bind(Exclude = "Id")] UserRegistrationModel model)
        {
            var user = UserManager.LoginUserWithEmail(db, model);

            if (user == null)
            {
                return Error("Invalid email / password");
            }

            user.GenerateNewSessionId();
            db.SaveChanges();

            return Json(user.SessionId, JsonRequestBehavior.AllowGet);
        }


        [AuthenticationFilter]
        public ActionResult MyInfo(bool isFirstLoad)
        {
            if (isFirstLoad)
            {
                BackgroundJob.Enqueue(() => new BackendController().RefreshFacebookInfo(currentUser.Id));
            }
            return Json(currentUser.DTO());
        }

        [AuthenticationFilter]
        public ActionResult Friends()
        {
            return Json(currentUser.Friends.Select(f => f.DTO()));
        }
    }
}