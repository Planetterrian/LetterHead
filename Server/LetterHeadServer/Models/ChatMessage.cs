using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using MyWebApplication;

namespace LetterHeadServer.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Index]
        public virtual User Sender { get; set; }

        [Index]
        public virtual User Reciever { get; set; }
        public DateTime SentOn { get; set; }
        public String Message { get; set; }

        [Index]
        public bool Read { get; set; }


        public LetterHeadShared.DTO.ChatMessage DTO()
        {
            var dto = Startup.Mapper.Map<LetterHeadShared.DTO.ChatMessage>(this);
            return dto;
        }

        public static int UnreadCount(ApplicationDbContext db, User currentUser, int fromUserId)
        {
            return db.ChatMessages.Count(m => !m.Read && m.Sender.Id == fromUserId && m.Reciever.Id == currentUser.Id);
        }
    }
}