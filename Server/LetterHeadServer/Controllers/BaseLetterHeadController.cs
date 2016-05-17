using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LetterHeadServer.Models;

namespace LetterHeadServer.Controllers
{
    public class BaseLetterHeadController : Controller
    {
        protected User currentUser;
        protected ApplicationDbContext db;

        protected BaseLetterHeadController()
        {
            db = ApplicationDbContext.Get();
        }

        public ActionResult Error(string message)
        {
            return Json(new
                        {
                            Error = message
                        });
        }

        public void SetCurrentUser(User user)
        {
            currentUser = user;
        }
    }
}