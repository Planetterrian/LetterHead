using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LetterHeadShared.DTO
{
    public class Match
    {
        public enum MatchState
        {
            Pregame, Running, Ended
        }

        public MatchState CurrentState;
        public List<string> Users;
        public List<MatchRound> Rounds;
        public string Letters;
        public int Id;
        public int RoundTimeSeconds;
        public int CurrentRoundNumber;
        public int MyUserId;
    }
}