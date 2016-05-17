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
        [Required(ErrorMessage = "The email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Display(Name = "Choose a Password:")]
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Password must be at least 6 characters", MinimumLength = 6)]
        public string Password { get; set; }

        [Display(Name = "Choose a Username:")]
        [Required(ErrorMessage = "Username is required")]
        [StringLength(16, ErrorMessage = "Username must be at least 3 characters", MinimumLength = 3)]
        public string Username { get; set; }

        public int ID { get; set; }
    }
}