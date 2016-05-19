using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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

                UserManager.CreateUser(db, model.Email, model.Password, model.Username);
                UserManager.LoginUserWithEmail(db, model);

                return Json("ok");
            }
            else
            {
                var errorList = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
                    );

                return Json(new
                            {
                                ErrorsList = errorList
                            });
            }
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

            return Json(new {SessionId = user.SessionId}, JsonRequestBehavior.AllowGet);
        }


        [AuthenticationFilter]
        public ActionResult MyInfo()
        {
            return Json(currentUser.DTO());
        }
    }
}