using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LetterHeadServer.Controllers
{
    public class HomeController : BaseLetterHeadController
    {
        public string Tip()
        {
            var tooltip = db.Tooltips.OrderBy(r => Guid.NewGuid()).First();
            return tooltip.Content;
        }
    }
}