﻿using System;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardWindow : WindowController
{
    public static LeaderboardWindow Instance;

    public Button previousButton;
    public Button nextButton;
    public TextMeshProUGUI dateString;
    public TextMeshProUGUI myRankLabel;
    public TextMeshProUGUI titleLabel;
    public GameObject rowPrefab;
    public Transform scrollParent;

    public CategoryManager.Type scoringType;

    private int dayOffset;

    protected override void Awake()
    {
        base.Awake();

        Instance = this;
    }

    private void OnWindowShown()
    {
        dateString.text = "";
        scrollParent.DeleteChildren();
        dayOffset = 0;
        titleLabel.text = "Leaderboard (" + GameManager.ModeName(scoringType) + ")";
        Refresh();
    }

    public void NextClicked()
    {
        dayOffset--;
        Refresh();
    }

    public void PreviousClicked()
    {
        dayOffset++;
        Refresh();
    }

    void OnWindowHidden()
    {
        MenuGui.Instance.loadingEffect.loading = false;
    }

    public class LeaderboardData
    {
        public List<LeaderboardWindow.LeaderboardRowData> Scores;
        public string DateString;
        public int TotalPlayers;
        public int MyRank;
    }


    public class LeaderboardRowData
    {
        public int Score;
        public int Rank;
        public string Username;
    }

    private void Refresh()
    {
        previousButton.interactable = false;
        nextButton.interactable = false;

        MenuGui.Instance.loadingEffect.loading = true;

        Srv.Instance.POST("Match/DailyLeaderbaord", new Dictionary<string, string>() { { "dayOffset", dayOffset.ToString()}, { "scoringType", scoringType.ToString() } }, s =>
        {
            scrollParent.DeleteChildren();

            var scores = JsonConvert.DeserializeObject<LeaderboardData>(s);
            dateString.text = scores.DateString;

            if (scores.MyRank > 0)
            {
                myRankLabel.text = "My Rank: " + scores.MyRank + "/" + scores.TotalPlayers.ToString("N0");
            }
            else
            {
                myRankLabel.text = scores.TotalPlayers.ToString("N0") + " Players";
            }

            var darkColor = false;
            foreach (var leaderboardRowData in scores.Scores)
            {
                var row = GameObject.Instantiate(rowPrefab);
                row.transform.SetParent(scrollParent);
                row.transform.ResetToOrigin();

                row.GetComponent<LeaderboardRow>().Set(leaderboardRowData, darkColor);
                darkColor = !darkColor;
            }

            OnLeaderboardRefreshed();
        }, s =>
        {
            OnLeaderboardRefreshed();

            scrollParent.DeleteChildren();

            var row = GameObject.Instantiate(rowPrefab);
            row.transform.SetParent(scrollParent);
            row.transform.ResetToOrigin();

            row.GetComponent<LeaderboardRow>().Set(null, false);
        }
        );
    }

    private void OnLeaderboardRefreshed()
    {
        MenuGui.Instance.loadingEffect.loading = false;
        previousButton.interactable = dayOffset < 8;
        nextButton.interactable = dayOffset > 0;
    }
}
