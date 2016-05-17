using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using LetterHeadServer.Models;

namespace LetterHeadServer.Classes
{
    public static class UserManager
    {
        private static string RememberSalt = "NomYaiiimak";
/*

        public static User LoginUserFromFacebook(ApplicationDbContext db, FacebookUser facebookUser)
        {
            var user = db.Users.SingleOrDefault(u => u.FacebookId == facebookUser.Id);

            if (user == null)
                user = CreateUser(db, facebookUser);

            user.facebookUser = facebookUser;

            DoLogin(user);

            return user;
        }
*/

        private static void DoLogin(User user)
        {
            HttpContext.Current.Session["UserId"] = user.Id;
        }
/*


        public static User LoginUserWithEmail(ApplicationDbContext db, UserRegistrationModel model)
        {
            var user = db.Users.SingleOrDefault(u => u.Email == model.Email);

            if (user != null)
            {
                if (Crypto.VerifyHashedPassword(user.PasswordHash, model.Password))
                {
                    DoLogin(user);

                    if (model.RememberMe)
                    {
                        var rememberSession = Crypto.SHA256((user.Id + user.PasswordHash + RememberSalt));
                        HttpContext.Current.Response.SetCookie(new HttpCookie("SessCrypt", rememberSession) { Expires = DateTime.Now.AddDays(60) });
                        HttpContext.Current.Response.SetCookie(new HttpCookie("SessUID", user.Id.ToString()) { Expires = DateTime.Now.AddDays(60) });
                    }
                    return user;
                }
            }

            return null;
        }
*/
/*

        private static User CreateUser(ApplicationDbContext db, FacebookUser facebookUser)
        {
            var user = new User()
            {
                FacebookId = facebookUser.Id,
                Tickets = 10,
                SignupDate = DateTime.Now,
            };

            db.Users.Add(user);
            db.SaveChanges();

            user.Log("Signup (Facebook)");

            return user;
        }
*/


        public static User CreateUser(ApplicationDbContext db, string email, string password)
        {
            var user = new User()
            {
                Email = email,
                PasswordHash = Crypto.HashPassword(password),
                SignupDate = DateTime.Now,
            };

            db.Users.Add(user);
            db.SaveChanges();

            return user;
        }


        public static User GetUser(ApplicationDbContext db, int userIdInt)
        {
            return db.Users.Find(userIdInt);
        }
/*

        internal static User GetUserByFacebookId(ApplicationDbContext db, string facebookId)
        {
            return db.Users.FirstOrDefault(u => u.FacebookId == facebookId);
        }
*/

        public static User GetUserBySession(ApplicationDbContext db, string sessionId)
        {
            return db.Users.FirstOrDefault(u => u.SessionId == sessionId);
        }
    }
}