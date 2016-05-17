using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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


    protected void LoadMatchDetails()
    {
        GameGui.Instance.shuffleButton.interactable = false;
        GameGui.Instance.startButton.interactable = false;

        Srv.Instance.POST("Match/MatchInfo", new Dictionary<string, string>() {{"matchId", MatchId.ToString()}}, MatchDetailsDownloaded);
    }

    protected virtual void MatchDetailsDownloaded(string matchDetailsJson)
    {
        MatchDetails = JsonConvert.DeserializeObject<Match>(matchDetailsJson);

        OnMatchDetailsLoaded();
    }

    protected virtual void OnMatchDetailsLoaded()
    {
        GameGui.Instance.shuffleButton.interactable = true;
        GameGui.Instance.startButton.interactable = true;
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
}
