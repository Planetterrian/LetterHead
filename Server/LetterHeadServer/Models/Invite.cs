using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using MyWebApplication;

namespace LetterHeadServer.Models
{
    public class Invite
    {
        public int Id { get; set; }

        public virtual User User { get; set; }

        public virtual User Inviter { get; set; }
        public DateTime InviteSentOn { get; set; }

        public LetterHeadShared.DTO.Invite DTO()
        {
            return Startup.Mapper.Map<LetterHeadShared.DTO.Invite>(this);
        }
    }
}