using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LetterHeadServer.Models
{
    public class MatchBuzz
    {
        public int Id { get; set; }

        [Required]
        public Match match { get; set; }
        public DateTime date { get; set; }
        public User SourceUser { get; set; }
        public User DestinationUser { get; set; }
    }
}