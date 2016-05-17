using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using LetterHeadServer.Controllers;
using LetterHeadServer.Models;

namespace LetterHeadServer.Classes
{
    public class AuthenticationFilterAttribute : ActionFilterAttribute
    {
        private bool _allowLoggedOut;
        public AuthenticationFilterAttribute(bool allowLoggedOut = false)
        {
            _allowLoggedOut = allowLoggedOut;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var db = ApplicationDbContext.Get();
            var controller = (BaseLetterHeadController)filterContext.Controller;
            if(controller == null)
                return;

            var sessionId = filterContext.HttpContext.Request.Headers["SessionId"];
            if (sessionId != null)
            {
                var user = UserManager.GetUserBySession(db, sessionId);
                if (user != null)
                {
                    LoginUser(filterContext, db, user.Id);
                    return;
                }
                else if(!_allowLoggedOut)
                {
                    InvalidSession(filterContext);
                    return;
                }
            }

            if(!_allowLoggedOut)
                InvalidSession(filterContext);
        }

        private static void InvalidSession(ActionExecutingContext filterContext)
        {
            var controller = (BaseLetterHeadController)filterContext.Controller;
            filterContext.Result = controller.Error("Invalid Session");
        }

        private void LoginUser(ActionExecutingContext filterContext, ApplicationDbContext db, int userId)
        {
            var controller = (BaseLetterHeadController)filterContext.Controller;

            var user = UserManager.GetUser(db, userId);
            if (user == null)
            {
                InvalidSession(filterContext);
                return;
            }
            
            //filterContext.Controller.ViewBag.User = user;
            controller.SetCurrentUser(user);
        }
    }
}