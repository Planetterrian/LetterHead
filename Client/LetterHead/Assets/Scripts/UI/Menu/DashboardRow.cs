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

    private void Refresh()
    {
        var score1 = MatchInfo.UserScore(0);
        var score2 = MatchInfo.UserScore(1);

        var opponentInfo = MatchInfo.Users.First(u => u.Id != ClientManager.Instance.UserId());
        usernameLabel.text = opponentInfo.Username;
        avatarBox.SetAvatarImage(opponentInfo.AvatarUrl);
        roundLabel.text = "Round " + (MatchInfo.CurrentRoundNumber + 1) + "/" + MatchInfo.TotalRoundsCount();
        myScoreLabel.text = "My Score: " + (MatchInfo.IndexOfUser(ClientManager.Instance.UserId()) == 0 ? score1.ToString() : score2.ToString());
        theirScoreLabel.text = "Their Score: " + (MatchInfo.IndexOfUser(ClientManager.Instance.UserId()) == 0 ? score2.ToString() : score1.ToString());
    }
}