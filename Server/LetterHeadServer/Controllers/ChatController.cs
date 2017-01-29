using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LetterHeadServer.Classes;
using LetterHeadServer.Models;

namespace LetterHeadServer.Controllers
{
    [AuthenticationFilter]
    public class ChatController : BaseLetterHeadController
    {
        public ActionResult UnreadMessageCount(int fromUserId)
        {
            var count = ChatMessage.UnreadCount(db, currentUser, fromUserId);
            return Json(count);
        }

        public ActionResult GetAllMessages(int fromUserId, int lastRecievedMessageId)
        {
            var oldest = DateTime.Now.Subtract(TimeSpan.FromDays(7));
            var messages =
                db.ChatMessages.Where(
                    m => m.Id > lastRecievedMessageId && 
                        ((m.Sender != null && m.Reciever != null && m.Sender.Id == fromUserId && m.Reciever.Id == currentUser.Id) ||
                        (m.Sender != null && m.Reciever != null && m.Sender.Id == currentUser.Id && m.Reciever.Id == fromUserId)) && m.SentOn > oldest).OrderBy(m => m.SentOn).ToList();


            foreach (var chatMessage in messages)
            {
                if(chatMessage.Reciever != null && chatMessage.Reciever.Id == currentUser.Id)
                    chatMessage.Read = true;
            }

            db.SaveChanges();

            return Json(messages.Select(m => m.DTO()).ToList());
        }

        public ActionResult SendMessage(int toUserId, string content)
        {
            var to = db.Users.Find(toUserId);
            if (to == null || to == currentUser)
                return Error("Invalid user");

            var message = new ChatMessage()
            {
                Sender = currentUser,
                Reciever = to,
                SentOn = DateTime.Now,
                Message = content
            };

            db.ChatMessages.Add(message);
            db.SaveChanges();
            return Okay();
        }
    }
}