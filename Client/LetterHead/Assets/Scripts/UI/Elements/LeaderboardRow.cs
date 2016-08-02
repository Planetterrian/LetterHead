using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardRow : MonoBehaviour
{
    public TextMeshProUGUI rank;
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI score;

    public Image background;

    public void Set(LeaderboardWindow.LeaderboardRowData leaderboardRowData, bool darkColor)
    {
        if (darkColor)
        {
            background.color = new Color(background.color.r, background.color.g, background.color.b, background.color.a + 0.1f);
        }

        if (leaderboardRowData == null)
        {
            rank.text = "No daily game found for that day.";
            playerName.text = "";
            score.text = "";
        }
        else
        {
            var isMine = leaderboardRowData.Username == ClientManager.Instance.myUserInfo.Username;

            var color = "";
            if (isMine)
                color = "<#449944>";

            rank.text = color + leaderboardRowData.Rank + ".";
            playerName.text = color + leaderboardRowData.Username;
            score.text = color + leaderboardRowData.Score.ToString("N0");
        }
    }
}