﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using LetterHeadServer.Classes;
using LetterHeadShared;

namespace LetterHeadServer.Models
{
    public class DailyGame
    {
        public int Id { get; set; }

        [Index]
        public User Winner { get; set; }
        public string LettersEncoded { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public CategoryManager.Type ScoringType { get; set; }

        public string RoundLetters(int roundNumber)
        {
            return LettersEncoded.Split(',')[roundNumber];
        }

        private const int RoundCount = 9;

        public static void CreateNewDailyGame(CategoryManager.Type scoringType)
        {
            var db = ApplicationDbContext.Get();

            var current = Current(scoringType, db);
            current?.End(db);

            var letterArray = new List<string>();

            for (int i = 0; i < RoundCount; i++)
            {
                letterArray.Add(BoardHelper.GenerateBoard());
            }

            db.DailyGames.Add(new DailyGame()
            {
                StartDate = DateTime.Now,
                LettersEncoded =  String.Join(",", letterArray),
                ScoringType = scoringType
            });

            db.SaveChanges();
        }

        public static DailyGame Current(CategoryManager.Type scoringType, ApplicationDbContext context = null)
        {
            var db = context ?? ApplicationDbContext.Get();

            return db.DailyGames.Where(m => m.ScoringType == scoringType).OrderByDescending(d => d.Id).FirstOrDefault();
        }

        private void End(ApplicationDbContext context)
        {
            EndDate = DateTime.Now;

            var topMatch = context.Matches.Where(m => m.DailyGame.Id == Id).OrderByDescending(m => m.DailyGame != null && m.DailyGame.Id == Id).FirstOrDefault();

            if (topMatch != null)
                Winner = topMatch.Users[0];

            context.SaveChanges();

            EndExistingGames(context);
        }

        private void EndExistingGames(ApplicationDbContext context)
        {
            var games = context.Matches.Where(m => m.DailyGame.Id == Id && m.CurrentState != LetterHeadShared.DTO.Match.MatchState.Ended).ToList();

            foreach (var game in games)
            {
                game.EndMatch();
            }

            context.SaveChanges();
        }

        public bool CanStart()
        {
            return (DateTime.Now - StartDate).TotalHours < 23.5;
        }

        public Match CreateMatchForUser(ApplicationDbContext context, User currentUser)
        {
            var rounds = (Environment.UserName == "Pete") ? 2 : 9;

            var match = Match.New(context, new List<User>(){ currentUser }, ScoringType, rounds);
            match.DailyGame = this;
            match.CurrentState = LetterHeadShared.DTO.Match.MatchState.Pregame;
                        context.SaveChanges();

            match.AddRounds(context);

            context.SaveChanges();

            var roundsList = match.Rounds.ToList();
            for (int index = 0; index < roundsList.Count; index++)
            {
                var round = roundsList[index];
                round.Letters = RoundLetters(index);
            }

            match.SetupUserOrder();

            context.SaveChanges();

            return match;
        }
    }
}