using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class SingleplayerGameManager : GameManager
{
    // Use this for initialization
    void Start ()
	{
        GameScene.Instance.CurrentState = GameScene.State.Pregame;

        Srv.Instance.POST("Match/RequestDailyGameStart", null, s =>
	    {
            MatchId = JsonConvert.DeserializeObject<int>(s);
	        LoadMatchDetails();
	    });

        GameGui.Instance.HidePowerups();
	}

    protected override void OnMatchDetailsLoaded()
    {
        base.OnMatchDetailsLoaded();

        BoardManager.Instance.SetBoardLetters(MatchDetails.Letters, true);
    }
}
