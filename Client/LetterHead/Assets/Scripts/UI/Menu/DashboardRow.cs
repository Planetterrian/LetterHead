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

    public Animator slideAnimator;

    [HideInInspector]
    public HomePage homePage;

    [HideInInspector]
    public bool markForDelete;

    private Match matchInfo;
    private bool backShown;

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
        }


        usernameLabel.text = opponentInfo.Username;
        avatarBox.SetAvatarImage(opponentInfo.AvatarUrl);
        roundLabel.text = "Round " + (MatchInfo.CurrentRoundNumber + 1) + "/" + MatchInfo.MaxRounds;
        myScoreLabel.text = "My Score: " + (MatchInfo.IndexOfUser(ClientManager.Instance.UserId()) == 0 ? score1.ToString() : score2.ToString());

        if (MatchInfo.Users.Count == 1)
        {
            theirScoreLabel.text = "";
            dailyGameLabel.gameObject.SetActive(true);
        }
        else
        {
            theirScoreLabel.text = "Their Score: " + (MatchInfo.IndexOfUser(ClientManager.Instance.UserId()) == 0 ? score2.ToString() : score1.ToString());
        }

    }
}