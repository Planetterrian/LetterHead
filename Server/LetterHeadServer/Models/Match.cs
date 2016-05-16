using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LetterHeadServer.Models
{
    public class Match
    {
        public int Id { get; set; }
        public List<User> Users { get; set; }
        public User Winner { get; set; }
        public State CurrentState { get; set; }
        public DailyGame DailyGame { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime StartedOn { get; set; }

        public enum State
        {
            Pregame, Running, Ended
        }
    }
}