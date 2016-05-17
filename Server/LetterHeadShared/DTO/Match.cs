using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LetterHeadShared.DTO
{
    public class Match
    {
        public enum State
        {
            Pregame, Running, Ended
        }

        public State CurrentState;
        public List<string> Users;
        public string Letters;
        public int Id;
        public int RoundTimeSeconds;
    }
}