using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LetterHeadShared;
using MyWebApplication;

namespace LetterHeadServer.Models
{
    public class MatchRound
    {
        public int Id { get; set; }
        public virtual Match Match { get; set; }
        public virtual User User { get; set; }
        public int Number { get; set; }
        public int Score { get; set; }
        public List<string> Words { get; set; }
        public string CategoryName { get; set; }
        public RoundState CurrentState { get; set; }

        public enum RoundState
        {
            NotStarted, Active, Ended
        }

        public Category Category(CategoryManager categoryManager)
        {
            return categoryManager.GetCategory(CategoryName);
        }

        public LetterHeadShared.DTO.MatchRound DTO()
        {
            return Startup.Mapper.Map<LetterHeadShared.DTO.MatchRound>(this);
        }
    }
}