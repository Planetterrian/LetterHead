using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LetterHeadServer.Models
{
    public class UserRegistrationModel
    {
        [Display(Name = "What's your Email:")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        public string Password { get; set; }

        public int ID { get; set; }
    }
}