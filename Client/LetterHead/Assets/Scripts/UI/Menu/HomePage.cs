﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared.DTO;
using Newtonsoft.Json;
using UI.Pagination;
using UnityEngine;
using UnityEngine.UI;

public class HomePage : Page
{
    public GameObject dashboardRowPrefab;
    public GameObject clearAllButton;

    public Transform myTurnHeader;
    public Transform theirTurnHeader;
    public Transform completedHeader;
    public WindowController tutorialWindow;
    public Toggle dontShowTutorialToggle;

    public Transform scrollParent;
    public float pollDelay;

    private List<DashboardRow> myMatchRows = new List<DashboardRow>(); 
    private List<DashboardRow> theirMatchRows = new List<DashboardRow>(); 
    private List<DashboardRow> completedMatchRows = new List<DashboardRow>();

    private List<Invite> invites = new List<Invite>();
    private bool matchesRefreshing;
    private float lastPoll;
    private bool refreshBars;

    void Start()
    {
        clearAllButton.SetActive(false);
        RefreshMatches();
    }

    public class ListInfo
    {
        public List<Match> Matches;
        public List<Invite> Invites;
        public bool[] CanDoDailyArray;
    }

    void OnApplicationFocus(bool state)
    {
        if (state)
        {
            RefreshMatches();
        }
    }

    public bool AreMatchesRefreshing()
    {
        return matchesRefreshing;
    }

    // For events to use
    public void RefreshMatches(bool showCog = false)
    {
        RefreshMatches(null, showCog);
    }

    public void RefreshMatches(Action refreshCallback, bool showCog = false)
    {
        if(matchesRefreshing)
            return;

        if(!ClientManager.Instance.PlayerDataLoaded())
            return;

        if (showCog)
        {
            MenuGui.Instance.loadingEffect.loading = true;
        }

        matchesRefreshing = true;
        lastPoll = Time.time;

        Srv.Instance.POST("Match/List", new Dictionary<string, string>() { { "scoringType", NewGamePage.ScoringType().ToString() } }, s =>
        {
            var list = JsonConvert.DeserializeObject<ListInfo>(s);
            var matches = list.Matches;
            invites = list.Invites;

            ClientManager.Instance.CanDoDaily = list.CanDoDailyArray;

            if(NewGamePage.Instance)
                NewGamePage.Instance.Refresh();

            var myMatches = matches.Where(m => m.CurrentState != Match.MatchState.Ended && m.CurrentUserId == ClientManager.Instance.myUserInfo.Id).ToList();
            var theirMatches = matches.Where(m => m.CurrentState != Match.MatchState.Ended && m.CurrentUserId != ClientManager.Instance.myUserInfo.Id).ToList();
            var completedMatches = matches.Where(m =>m.CurrentState == Match.MatchState.Ended).ToList();

            UpdateMatchSet(myMatchRows, myMatches, myTurnHeader, DashboardRow.RowType.MyTurn);
            UpdateMatchSet(theirMatchRows, theirMatches, theirTurnHeader, DashboardRow.RowType.TheirTurn);
            UpdateMatchSet(completedMatchRows, completedMatches, completedHeader, DashboardRow.RowType.Completed);

            clearAllButton.SetActive(completedMatches.Count > 0);

            matchesRefreshing = false;

            if (showCog)
            {
                MenuGui.Instance.loadingEffect.loading = false;
            }

            if (refreshCallback != null)
                refreshCallback();

        }, s =>
        {
            matchesRefreshing = false;

            if (showCog)
            {
                MenuGui.Instance.loadingEffect.loading = false;
            }
        });
    }

    public void ClearMatches()
    {
        foreach (var dashboardRow in myMatchRows)
        {
            GameObject.Destroy(dashboardRow.gameObject);
        }
        foreach (var dashboardRow in theirMatchRows)
        {
            GameObject.Destroy(dashboardRow.gameObject);
        }
        foreach (var dashboardRow in completedMatchRows)
        {
            GameObject.Destroy(dashboardRow.gameObject);
        }

        myMatchRows.Clear();
        theirMatchRows.Clear();
        completedMatchRows.Clear();
        clearAllButton.SetActive(false);
    }

    public override void OnShow()
    {
        base.OnShow();

        AchievementManager.Instance.CheckServerAchievements();

        if (!TutorialShown() || (TutorialShown() && PlayerPrefs.GetInt("DontShowTutorialToggle" + ClientManager.Instance.UserId(), 0) == 0) && ClientManager.Instance.IsNewLogin)
        {
            ShowTutorial();
        }

        ClientManager.Instance.IsNewLogin = false;
    }

    public void CloseTutorial()
    {
        PlayerPrefs.SetInt("DontShowTutorialToggle" + ClientManager.Instance.UserId(), dontShowTutorialToggle.isOn ? 1: 0);
        tutorialWindow.Hide();
    }

    private bool TutorialShown()
    {
        return PlayerPrefs.GetInt("TutShown" + ClientManager.Instance.UserId(), 0) == 1;
    }

    public void ShowTutorial()
    {
        dontShowTutorialToggle.isOn = PlayerPrefs.GetInt("DontShowTutorialToggle" + ClientManager.Instance.UserId(), 0) == 1;

        PlayerPrefs.SetInt("TutShown" + ClientManager.Instance.UserId(), 1);
        tutorialWindow.ShowModal();
        tutorialWindow.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 1;
    }

    void Update()
    {
        if (Time.time - lastPoll > pollDelay)
        {
            RefreshMatches();
        }

        if (invites.Count > 0 && !DialogWindowTM.Instance.gameObject.activeInHierarchy)
        {
            var invite = invites[0];
            invites.RemoveAt(0);

            SoundManager.Instance.PlayClip("Invite");

            DialogWindowTM.Instance.Show("Invite", "You have a match invite from " + invite.Inviter.Username + " (" + invite.ScoringType.ToString() + "). Do you want to accept?",
                () =>
                {
                    Srv.Instance.POST("Match/AcceptInvite",
                        new Dictionary<string, string>() {{"inviteId", invite.Id.ToString()}},
                        s =>
                        {
                            RefreshMatches();
                        }, DialogWindowTM.Instance.Error);
                }, () =>
                {
                    Srv.Instance.POST("Match/DeclineInvite",
                        new Dictionary<string, string>() {{"inviteId", invite.Id.ToString()}}, s =>
                        {
                        });
                }, "Accept", "Decline");
        }

        if (refreshBars)
        {
            RefreshBars(theirMatchRows);
            RefreshBars(myMatchRows);
            RefreshBars(completedMatchRows);
            refreshBars = false;
        }
    }

    private void RefreshBars(List<DashboardRow> rows)
    {
        var sortedRows = rows.OrderByDescending(r => r.transform.position.y).ToList();

        for (int index = 0; index < sortedRows.Count; index++)
        {
            var dashboardRow = sortedRows[index];

            if (index == sortedRows.Count - 1)
                dashboardRow.barBottom.SetActive(true);
            else
                dashboardRow.barBottom.SetActive(false);
        }

    }

    private void UpdateMatchSet(List<DashboardRow> rows, List<Match> matches, Transform header, DashboardRow.RowType rowType)
    {
        // Update and remove existing
        for (int index = rows.Count - 1; index >= 0; index--)
        {
            var dashboardRow = rows[index];
            var match = matches.FirstOrDefault(m => m.Id == dashboardRow.MatchInfo.Id);

            if (match != null)
            {
                dashboardRow.MatchInfo = match;
            }
            else
            {
                GameObject.Destroy(dashboardRow.gameObject);
                rows.RemoveAt(index);
            }
        }

        foreach (var match in matches)
        {
            var row = rows.FirstOrDefault(r => r.MatchInfo.Id == match.Id);
            if (row == null)
            {
                var rowGo = GameObject.Instantiate(dashboardRowPrefab) as GameObject;
                rowGo.transform.SetParent(scrollParent);
                rowGo.transform.ResetToOrigin();

                var parentIndex = header.GetSiblingIndex();
                rowGo.transform.SetSiblingIndex(parentIndex + 1);

                row = rowGo.GetComponent<DashboardRow>();
                row.homePage = this;
                row.type = rowType;
                row.MatchInfo = match;
                rows.Add(row);
            }
        }

        refreshBars = true;
    }

    public void OnRowClicked(DashboardRow dashboardRow)
    {
        PersistManager.Instance.LoadMatch(dashboardRow.MatchInfo.Id, dashboardRow.MatchInfo.Users.Count == 1);
    }

    public void ResignMatch(Match matchInfo)
    {
        Srv.Instance.POST("Match/Resign", new Dictionary<string, string>() {{"matchId", matchInfo.Id.ToString()}}, s =>
        {
            RefreshMatches();
        });
    }

    public void ClearMatch(Match matchInfo)
    {
        MenuGui.Instance.loadingEffect.loading = true;

        Srv.Instance.POST("Match/Clear", new Dictionary<string, string>() { { "matchId", matchInfo.Id.ToString() } }, s =>
        {
            RefreshMatches(true);
        }, s =>
        {
            MenuGui.Instance.loadingEffect.loading = false;
        });
    }

    public void ClearAllMatches()
    {
        DialogWindowTM.Instance.Show("Clear Matches", "Are you sure you want to clear all completed matches?", () =>
        {
            DialogWindowTM.Instance.Show("Clear Matches", "Clearing matches...", () => { },
                () => { }, "");

            Srv.Instance.POST("Match/ClearAll", null, s =>
            {
                if (AreMatchesRefreshing())
                {
                    DialogWindowTM.Instance.Hide();
                }
                else
                {
                    RefreshMatches(() =>
                    {
                        DialogWindowTM.Instance.Hide();
                    });
                }
            });
        }, () => { });

    }

    public void BuzzMatch(Match matchInfo)
    {
        Srv.Instance.POST("Match/Buzz", new Dictionary<string, string>() { { "matchId", matchInfo.Id.ToString() } }, s =>
        {
            DialogWindowTM.Instance.Show("Poke", "You poked your opponent.", () => { });
        }, DialogWindowTM.Instance.Error);
    }

    public void Rematch(Match matchInfo)
    {
        var opponentInfo = matchInfo.Users.FirstOrDefault(u => u.Id != ClientManager.Instance.UserId());

        DialogWindowTM.Instance.Show("Invite", "Sending invite to " + opponentInfo.Username + ".", () => { }, () => { }, "");
        Srv.Instance.POST("Match/Invite", new Dictionary<string, string>() { { "userId", opponentInfo.Id.ToString() }, { "scoringType", matchInfo.ScoringType.ToString() } },
            (s) =>
            {
                DialogWindowTM.Instance.Show("Invite", "Invite sent!", () => { });
            }, DialogWindowTM.Instance.Error);
    }
}