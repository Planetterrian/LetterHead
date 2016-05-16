using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LetterHeadServer.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public DateTime SignupDate { get; set; }
    }
}