using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LetterHeadServer.Controllers
{
    public class HomeController : Controller
    {
        public string Tip()
        {
            var tips = new []
            {
                "tip",
                "tip2",
                "tip3",
                "tip4",
                "tip5",
            };

            var rand = new Random();
            var tip = tips[rand.Next(tips.Length)];

            return tip;
        }
    }
}