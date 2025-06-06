﻿using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class SingleplayerGameManager : GameManager
{
    // Use this for initialization
    void Start ()
	{
        GameScene.Instance.CurrentState = GameScene.State.Pregame;

        if (PersistManager.Instance.matchToLoadId <= 0)
        {
            ClientManager.Instance.RefreshMyInfo(false, (b) =>
            {
                if (b)
                {
                    Srv.Instance.POST("Match/RequestDailyGameStart", null, s =>
                    {
                        MatchId = JsonConvert.DeserializeObject<int>(s);
                        LoadMatchDetails();
                    });
                }
            });
        }
        else
        {
            MatchId = PersistManager.Instance.matchToLoadId;
            LoadMatchDetails();
        }
	}

    protected override void OnMatchDetailsLoaded()
    {
        base.OnMatchDetailsLoaded();

        BoardManager.Instance.SetBoardLetters(CurrentRound().Letters, true);
    }
}
