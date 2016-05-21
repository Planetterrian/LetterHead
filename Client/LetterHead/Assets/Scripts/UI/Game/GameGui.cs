using System;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameGui : Singleton<GameGui>
{
    public Button submitWordButton;
    public Button clearWordButton;
    public Button shuffleButton;
    public Button startButton;
    public GameObject selectCategoryHelper;
    public TextMeshProUGUI roundNumberLabel;

    public TimerElement timer;

    public AvatarBox leftAvatarBox;
    public AvatarBox rightAvatarBox;

    public GameObject leftPowerupsBox;
    public GameObject rightPowerupsBox;

    private void Start()
    {
        GameManager.Instance.OnMatchDetailsLoadedEvent.AddListener(OnMatchDetailsLoaded);

        timer.OnTimeExpired.AddListener(OnTimeExpired);
    }

    public bool CanClickBoardTile()
    {
        return GameScene.Instance.CurrentState == GameScene.State.Active;
    }

    public void OnRealTimeConnected()
    {
        OnGameStateChanged();
    }

    public bool CanClickSpellerTile()
    {
        return true;
    }

    public void OnStartClicked()
    {
        GameRealTime.Instance.RequestStart();
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
            selectCategoryHelper.gameObject.SetActive(false);

            if (GameManager.Instance.CanStart())
                startButton.interactable = true;

        }
        else if (GameScene.Instance.CurrentState == GameScene.State.Active)
        {
            startButton.interactable = false;
            shuffleButton.interactable = true;
        }
        else if (GameScene.Instance.CurrentState == GameScene.State.End)
        {
            startButton.interactable = false;
            shuffleButton.interactable = false;
            selectCategoryHelper.gameObject.SetActive(false);
        }
        else if (GameScene.Instance.CurrentState == GameScene.State.WaitingForCategory)
        {
            startButton.interactable = false;
            shuffleButton.interactable = false;
            selectCategoryHelper.gameObject.SetActive(true);
        }
    }


    private void OnTimeExpired()
    {

    }

    public static bool CanSelectCategory()
    {
        if (GameScene.Instance.CurrentState == GameScene.State.Pregame || GameScene.Instance.CurrentState == GameScene.State.End)
            return false;

        if (!GameManager.Instance.IsMyRound())
            return false;

        if (GameManager.Instance.MyCurrentRound().CurrentState == MatchRound.RoundState.Ended || GameManager.Instance.MyCurrentRound().CurrentState == MatchRound.RoundState.NotStarted)
            return false;

        return true;
    }

    public void OnMatchDetailsLoaded()
    {
        OnGameStateChanged();

        SetAvatarBox(leftAvatarBox, 0);
        timer.SetTimer(GameManager.Instance.MatchDetails.RoundTimeSeconds);
        roundNumberLabel.text = "Round " + (GameManager.Instance.MatchDetails.CurrentRoundNumber + 1) + "/" + GameManager.Instance.MatchDetails.TotalRoundsCount();

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

    public void HidePowerups()
    {
        leftPowerupsBox.SetActive(false);
        rightPowerupsBox.SetActive(false);
    }
}