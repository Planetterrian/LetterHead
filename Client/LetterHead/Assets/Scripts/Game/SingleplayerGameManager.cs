using UnityEngine;
using System.Collections;

public class SingleplayerGameManager : GameManager
{

	// Use this for initialization
	void Start ()
	{
	    StartGame();
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
