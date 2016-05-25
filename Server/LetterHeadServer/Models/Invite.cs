using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LetterHeadServer.Models
{
    public class Invite
    {
        public int Id { get; set; }

        public User User { get; set; }

        public User Inviter { get; set; }
        public DateTime InviteSentOn { get; set; }
    }
}