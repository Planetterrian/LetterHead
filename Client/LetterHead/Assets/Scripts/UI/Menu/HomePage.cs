using System;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared.DTO;
using Newtonsoft.Json;
using UI.Pagination;
using UnityEngine;

public class HomePage : Page
{
    public GameObject dashboardRowPrefab;

    public Transform myTurnHeader;
    public Transform theirTurnHeader;
    public Transform completedHeader;

    public Transform scrollParent;

    private List<DashboardRow> myMatchRows = new List<DashboardRow>(); 
    private List<DashboardRow> theirMatchRows = new List<DashboardRow>(); 
    private List<DashboardRow> completedMatchRows = new List<DashboardRow>();

    void Start()
    {
        //RefreshMatches();
    }

    public void RefreshMatches()
    {
        Srv.Instance.POST("Match/List", null, s =>
        {
            var matches = JsonConvert.DeserializeObject<List<Match>>(s);

            var myMatches = matches.Where(m => m.CurrentState != Match.MatchState.Ended && m.Users[m.CurrentUserIndex].Id == ClientManager.Instance.myUserInfo.Id).ToList();
            var theirMatches = matches.Where(m => m.CurrentState != Match.MatchState.Ended && m.Users[m.CurrentUserIndex].Id != ClientManager.Instance.myUserInfo.Id).ToList();
            var completedMatches = matches.Where(m => m.CurrentState == Match.MatchState.Ended).ToList();

            UpdateMatchSet(myMatchRows, myMatches, myTurnHeader, DashboardRow.RowType.MyTurn);
            UpdateMatchSet(theirMatchRows, theirMatches, theirTurnHeader, DashboardRow.RowType.TheirTurn);
            UpdateMatchSet(completedMatchRows, completedMatches, completedHeader, DashboardRow.RowType.Completed);
        });
    }

    void OnShown()
    {
        RefreshMatches();
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
    }

    public void OnRowClicked(DashboardRow dashboardRow)
    {
        PersistManager.Instance.LoadMatch(dashboardRow.MatchInfo.Id, dashboardRow.MatchInfo.Users.Count == 1);
    }
}