using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LetterHeadServer.Models
{
    public class Session
    {
        public int Id { get; set; }

        public virtual User User { get; set; }

        [Index]
        [MaxLength(64)]
        public string SessionId { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime LastLoggedIn { get; set; }
    }
}