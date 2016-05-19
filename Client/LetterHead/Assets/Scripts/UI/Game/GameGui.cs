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

    public AvatarBox leftAvatarBox;
    public AvatarBox rightAvatarBox;

    private void Start()
    {
        GameManager.Instance.OnMatchDetailsLoadedEvent.AddListener(OnMatchDetailsLoaded);
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
        ScoringManager.Instance.OnWordSubmit();
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
        else if (GameScene.Instance.CurrentState == GameScene.State.Active)
        {
            startButton.interactable = false;
            shuffleButton.interactable = true;
            timer.StartTimer(GameManager.Instance.MatchDetails.RoundTimeSeconds);
        }
    }


    public static bool CanSelectCategory()
    {
        if (GameScene.Instance.CurrentState == GameScene.State.Active)
            return true;

        return false;
    }

    public void OnMatchDetailsLoaded()
    {
        startButton.interactable = true;

        SetAvatarBox(leftAvatarBox, 0);

        if (GameManager.Instance.MatchDetails.Users.Count > 1)
        {
            rightAvatarBox.gameObject.SetActive(true);
            SetAvatarBox(rightAvatarBox, 1);
        }
        else
        {
            rightAvatarBox.gameObject.SetActive(false);
        }
    }

    private void SetAvatarBox(AvatarBox box, int userIndex)
    {
        box.score.text = GameManager.Instance.MatchDetails.UserScore(userIndex).ToString("N0");
        box.SetAvatarImage(GameManager.Instance.MatchDetails.Users[userIndex].AvatarUrl);
        box.SetName(GameManager.Instance.MatchDetails.Users[userIndex].Username);
    }
}