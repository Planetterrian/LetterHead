using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Facebook;
using Hangfire;
using LetterHeadServer.Classes;
using LetterHeadServer.Models;

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

            actionResult = null;
            return false;
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
    }
}