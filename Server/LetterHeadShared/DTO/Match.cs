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
        public List<int> Scores;
        public List<MatchRound> Rounds;
        public int RoundTimeSeconds;
        public int CurrentRoundNumber;
        public int CurrentUserId;
        public int MaxRounds;


        public int UserScore(int userIndex)
        {
            return Scores[userIndex];
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

        public bool HasShieldBeenUsed(int userId)
        {
            return Rounds.Any(r => r.ShieldUsed && r.UserId == userId);
        }

        public bool HasDoOverBeenUsed(int userId)
        {
            return Rounds.Any(r => r.DoOverUsed && r.UserId == userId);
        }

        public bool HasStealTimeBeenUsed(int userId)
        {
            return Rounds.Any(r => r.StealTimeUsed && r.UserId == userId);
        }

        public bool HasStealLetterBeenUsed(int userId)
        {
            return Rounds.Any(r => r.StealLetterUsed && r.UserId == userId);
        }
    }

}