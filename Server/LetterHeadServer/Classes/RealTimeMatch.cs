using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LetterHeadServer.Classes
{
    public class RealTimeMatch
    {
        public Action<Message> OnNewMessage;
        public List<Object> Listeners { get; private set; } = new List<object>();

        public void AddMessage(Message message)
        {
            OnNewMessage?.Invoke(message);
        }

        public class Message
        {
            public Action Execute;

            public Message(Action method)
            {
                Execute = method;
            }
        }

    }
}