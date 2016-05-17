using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameGui : Singleton<GameGui>
{
    public Button submitWordButton;
    public Button clearWordButton;
    public Button shuffleButton;
    public Button startButton;

    public TimerElement timer;

    private void Start()
    {
    }

    public bool CanClickBoardTile()
    {
        return GameScene.Instance.CurrentState == GameScene.State.Active;
    }

    public bool CanClickSpellerTile()
    {
        return true;
    }

    public void OnStartClicked()
    {
        GameManager.Instance.StartGame();
    }

    public void ShuffleClicked()
    {
        BoardManager.Instance.Shuffle();
    }

    public void SubmitClicked()
    {
        GameScene.Instance.OnWordSubmit();
    }

    public void ClearClicked()
    {
        BoardManager.Instance.ClearCurrentWord();
    }

    public void OnGameStateChanged()
    {
        if (GameScene.Instance.CurrentState == GameScene.State.Pregame)
        {
            startButton.interactable = false;
            shuffleButton.interactable = false;
            startButton.interactable = false;
        }
        if (GameScene.Instance.CurrentState == GameScene.State.Active)
        {
            startButton.interactable = false;
            shuffleButton.interactable = true;
            timer.StartTimer(GameManager.Instance.MatchDetails.RoundTimeSeconds);
        }
    }


}