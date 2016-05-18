using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared;
using LetterHeadShared.DTO;
using Newtonsoft.Json;

public abstract class GameManager : Singleton<GameManager>
{
    protected GameScene gameScene;

    private int matchId;

    protected Match matchDetails;
    public Match MatchDetails
    {
        get { return matchDetails; }
        set { matchDetails = value; }
    }

    public int MatchId
    {
        get
        {
            return matchId;
        }

        set
        {
            matchId = value;
        }
    }

    public MatchRound MyCurrentRound()
    {
        return MatchDetails.Rounds.FirstOrDefault(m => m.UserId == MatchDetails.MyUserId && m.Number == MatchDetails.CurrentRoundNumber);
    }

    public List<MatchRound> MyRounds()
    {
        if(MatchDetails == null)
            return new List<MatchRound>();

        return MatchDetails.Rounds.Where(m => m.UserId == MatchDetails.MyUserId).ToList();
    }

    protected void LoadMatchDetails()
    {
        Srv.Instance.POST("Match/MatchInfo", new Dictionary<string, string>() {{"matchId", MatchId.ToString()}}, MatchDetailsDownloaded);
    }

    protected virtual void MatchDetailsDownloaded(string matchDetailsJson)
    {
        MatchDetails = JsonConvert.DeserializeObject<Match>(matchDetailsJson);

        OnMatchDetailsLoaded();
    }

    protected virtual void OnMatchDetailsLoaded()
    {
        GameGui.Instance.startButton.interactable = true;
        CategoryBox.Instance.Refresh();
    }

    protected override void Awake()
    {
        base.Awake();

        gameScene = GameScene.Instance;
    }
    
    public virtual void OnGameStateChanged()
    {
        
    }


    protected virtual void OnGameStarted()
    {
        gameScene.CurrentState = GameScene.State.Active;
        BoardManager.Instance.RevealLetters();
    }

    public void StartGame()
    {
        OnGameStarted();
    }

    public bool HasCategoryBeenUsed(Category category)
    {
        return MyRounds().Any(c => c.CategoryName == category.name);
    }

    public void SelectCategory(Category category)
    {
        ScoringManager.Instance.OnCategorySelected(category);
        CategoryBox.Instance.Refresh();
        CategoryBox.Instance.SetCurrentlySelectedCategory(category);
        
    }
}
