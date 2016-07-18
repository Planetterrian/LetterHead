using System;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared.DTO;
using TMPro;
using UI.Pagination;
using UnityEngine;
using UnityEngine.UI;

public class DashboardRow : MonoBehaviour
{
    public AvatarBox avatarBox;
    public TextMeshProUGUI usernameLabel;
    public TextMeshProUGUI roundLabel;
    public TextMeshProUGUI myScoreLabel;
    public TextMeshProUGUI theirScoreLabel;
    public TextMeshProUGUI dailyGameLabel;
    public TextMeshProUGUI soloGameLabel;
    public TextMeshProUGUI lastTurnLabel;

    public GameObject barBottom;

    public GameObject backBoxMy;
    public GameObject backBoxTheir;
    public GameObject backBoxComplete;

    public GameObject rematchButton;


    public Animator slideAnimator;

    [HideInInspector]
    public HomePage homePage;

    [HideInInspector]
    public bool markForDelete;

    private Match matchInfo;
    private bool backShown;

    public enum RowType
    {
        MyTurn, TheirTurn, Completed
    }

    public RowType type;

    public Match MatchInfo
    {
        get { return matchInfo; }
        set
        {
            matchInfo = value;
            Refresh();
        }
    }

    void Start()
    {
        GetComponent<SwipeDetector>().scrollRect = homePage.GetComponent<ScrollRect>();
    }

    public void OnSwipe(bool isRight)
    {
        if (!isRight && !backShown)
        {
            ShowBackBox();
        }
        else if (isRight && backShown)
        {
            HideBackBox();
        }
    }

    public void OnResignClicked()
    {
        DialogWindowTM.Instance.Show("Resign", "Are you sure you want to resign from this game?", () =>
        {
            homePage.ResignMatch(MatchInfo);
        }, () => { }, "Resign");
    }

    public void OnPokeClicked()
    {
        homePage.BuzzMatch(matchInfo);
    }

    public void OnClearClicked()
    {
        homePage.ClearMatch(MatchInfo);
    }

    public void OnRematchClicked()
    {
        homePage.Rematch(MatchInfo);
    }

    private void ShowBackBox()
    {
        backShown = true;
        slideAnimator.SetInteger("ShowBack", 1);
    }

    private void HideBackBox()
    {
        backShown = false;
        slideAnimator.SetInteger("ShowBack", 0);
    }

    public void OnClicked()
    {
        homePage.OnRowClicked(this);
    }

    private bool DidWin()
    {
        var score1 = MatchInfo.UserScore(0);
        var score2 = MatchInfo.UserScore(1);

        var isFirstPlayer = MatchInfo.IndexOfUser(ClientManager.Instance.UserId()) == 0;

        if (MatchInfo.ResignerUserId > 0)
            return ClientManager.Instance.UserId() != MatchInfo.ResignerUserId;

        if (isFirstPlayer)
        {
            return score1 > score2;
        }

        return score2 > score1;
    }

    private void Refresh()
    {
        var score1 = MatchInfo.UserScore(0);
        var score2 = 0;

        UserInfo opponentInfo;

        if (MatchInfo.Users.Count > 1)
        {
            score2 = MatchInfo.UserScore(1);
            opponentInfo = MatchInfo.Users.First(u => u.Id != ClientManager.Instance.UserId());
        }
        else
        {
            opponentInfo = MatchInfo.Users[0];
            rematchButton.SetActive(false);
        }

        backBoxMy.SetActive(type == RowType.MyTurn);
        backBoxTheir.SetActive(type == RowType.TheirTurn);
        backBoxComplete.SetActive(type == RowType.Completed);

        var isFirstPlayer = MatchInfo.IndexOfUser(ClientManager.Instance.UserId()) == 0;

        lastTurnLabel.text = MatchInfo.LastTurnInfo;
        usernameLabel.text = opponentInfo.Username;
        avatarBox.SetAvatarImage(opponentInfo.AvatarUrl);
        roundLabel.text = "Round " + (MatchInfo.CurrentRoundNumber + 1) + "/" + MatchInfo.MaxRounds;

        if (type == RowType.Completed)
        {
            if (MatchInfo.Users.Count > 1)
            {
                if (DidWin())
                    lastTurnLabel.text = "<color=#61BC6D>You Won";
                else
                    lastTurnLabel.text = "You Lost";
            }
        }

        myScoreLabel.text = "My Score: " + (isFirstPlayer ? score1.ToString() : score2.ToString());
        if (ClientManager.Instance.UserId() == matchInfo.ResignerUserId)
            myScoreLabel.text += " <size=80%>(Resigned)</size>";

        if (MatchInfo.Users.Count == 1)
        {
            theirScoreLabel.text = "";

            if (MatchInfo.IsDaily)
            {
                dailyGameLabel.gameObject.SetActive(true);
                dailyGameLabel.text = "DAILY GAME - " + MatchInfo.DateStringShort;
            }
            else
                soloGameLabel.gameObject.SetActive(true);
        }
        else
        {
            theirScoreLabel.text = "Their Score: " + (isFirstPlayer ? score2.ToString() : score1.ToString());

            if (opponentInfo.Id == matchInfo.ResignerUserId)
                theirScoreLabel.text += " <size=80%>(Resigned)</size>";
        }

    }
}