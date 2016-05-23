using System;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared.DTO;
using TMPro;
using UnityEngine;

public class DashboardRow : MonoBehaviour
{
    public AvatarBox avatarBox;
    public TextMeshProUGUI usernameLabel;
    public TextMeshProUGUI roundLabel;
    public TextMeshProUGUI myScoreLabel;
    public TextMeshProUGUI theirScoreLabel;

    public HomePage homePage;

    [HideInInspector]
    public bool markForDelete;

    private Match matchInfo;

    public Match MatchInfo
    {
        get { return matchInfo; }
        set
        {
            matchInfo = value;
            Refresh();
        }
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
        }
        else
        {
            theirScoreLabel.text = "Their Score: " + (MatchInfo.IndexOfUser(ClientManager.Instance.UserId()) == 0 ? score2.ToString() : score1.ToString());
        }

    }
}