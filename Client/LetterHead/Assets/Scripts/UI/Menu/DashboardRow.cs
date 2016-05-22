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

    public void Set(Match matchInfo)
    {
        this.matchInfo = matchInfo;

        Refresh();
    }

    private void Refresh()
    {
        var score1 = matchInfo.UserScore(0);
        var score2 = matchInfo.UserScore(1);

        var opponentInfo = matchInfo.Users.First(u => u.Id != ClientManager.Instance.UserId());
        usernameLabel.text = opponentInfo.Username;
        avatarBox.SetAvatarImage(opponentInfo.AvatarUrl);
        roundLabel.text = "Round " + (matchInfo.CurrentRoundNumber + 1) + "/" + matchInfo.TotalRoundsCount();
        myScoreLabel.text = "My Score: " + (matchInfo.IndexOfUser(ClientManager.Instance.UserId()) == 0 ? score1.ToString() : score2.ToString());
        theirScoreLabel.text = "Their Score: " + (matchInfo.IndexOfUser(ClientManager.Instance.UserId()) == 0 ? score2.ToString() : score1.ToString());
    }
}