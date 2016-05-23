using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LetterHeadServer.Models;

namespace LetterHeadServer.Classes
{
    public static class RealTimeMatchManager
    {
        private static Dictionary<int, RealTimeMatch> matches = new Dictionary<int, RealTimeMatch>();

        public static RealTimeMatch GetMatch(int matchId)
        {
            if (matches.ContainsKey(matchId))
            {
                return matches[matchId];
            }

            var match = new RealTimeMatch();
            matches[matchId] = match;

            return match;
        }
    }
}