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
            Pregame, Running, Ended, WaitingForPlayers
        }

        public int Id;
        public MatchState CurrentState;
        public List<UserInfo> Users;
        public List<MatchRound> Rounds;
        public string Letters;
        public int RoundTimeSeconds;
        public int CurrentRoundNumber;
        public int CurrentUserIndex;

        public int UserScore(int userIndex)
        {
            return Rounds.Where(r => r.UserId == Users[userIndex].Id).Sum(r => r.Score);
        }

        public int TotalRoundsCount()
        {
            return Rounds.Count(r => r.UserId == Users[0].Id);
        }

        public int IndexOfUser(int userId)
        {
            for (int index = 0; index < Users.Count; index++)
            {
                var userInfo = Users[index];

                if (userInfo.Id == userId)
                    return index;
            }

            return -1;
        }
    }
}