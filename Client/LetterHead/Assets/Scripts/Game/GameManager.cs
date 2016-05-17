using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class GameManager : Singleton<GameManager>
{
    protected GameScene gameScene;

    private int matchId;
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
        Srv.Instance.POST("Match/MatchInfo", new Dictionary<string, string>() {{"matchId", MatchId.ToString()}}, OnMatchDetailsLoaded);
    }

    private void OnMatchDetailsLoaded(string matchDetailsJson)
    {
        //var matchDetails = 
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
    }
}
