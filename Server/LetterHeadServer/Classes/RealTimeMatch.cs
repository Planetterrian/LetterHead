using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using LetterHeadServer.Controllers;

namespace LetterHeadServer.Classes
{
    public class RealTimeMatch
    {
        public List<RealTimeListener> Listeners { get; private set; } = new List<RealTimeListener>();


        public void AddMessage(Message message)
        {
            foreach (var listener in Listeners)
            {
                listener.messages.Add(message);
                listener.onMessageResetEvent.Set();
            }
        }

        public void AddMessage(Message message, RealTimeListener target)
        {
            target.messages.Add(message);
            target.onMessageResetEvent.Set();
        }

        public void RemoveListener(RealTimeController realTimeController)
        {
            Listeners.RemoveAll(r => r.realTimeController == realTimeController);
        }


        public RealTimeListener AddListener(RealTimeController obj)
        {
            var listener = new RealTimeListener()
                           {
                               realTimeController = obj,
                               onMessageResetEvent = new AutoResetEvent(false)
                           };

            Listeners.Add(listener);

            return listener;
        }

        public class RealTimeListener
        {
            public RealTimeController realTimeController;
            public AutoResetEvent onMessageResetEvent;
            public List<Message> messages = new List<Message>();


            public async Task<Message> ReceiveMessage()
            {
                await Task.Run(() =>
                {
                    onMessageResetEvent.WaitOne();
                });

                var message = messages[0];
                messages.RemoveAt(0);

                if (messages.Count > 0)
                    onMessageResetEvent.Set();

                return message;
            }
        }
        public class Message
        {
            public Action<RealTimeController> Payload;

            public Message(Action<RealTimeController> method)
            {
                Payload = method;
            }
        }
    }

}