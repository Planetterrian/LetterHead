using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class SingleplayerGameManager : GameManager
{
    // Use this for initialization
    void Start ()
	{
	    Srv.Instance.POST("Match/RequestDailyGameStart", null, s =>
	    {
            MatchId = JsonConvert.DeserializeObject<int>(s);
	        LoadMatchDetails();
	    });
	}


    private void StartGame()
    {
        BoardManager.Instance.GenerateRandomBoard();
        OnGameStarted();
    }

    protected override void OnGameStarted()
    {
        base.OnGameStarted();
    }
}
