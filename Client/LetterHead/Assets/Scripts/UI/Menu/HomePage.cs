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

    private List<DashboardRow> myMatchRows; 
    private List<DashboardRow> theirMatchRows; 
    private List<DashboardRow> completedMatchRows;

    public void RefreshMatches()
    {
        Srv.Instance.POST("Match/List", null, s =>
        {
            var matches = JsonConvert.DeserializeObject<List<Match>>(s);

            var myMatches = matches.Where(m => m.CurrentState != Match.MatchState.Ended && m.Users[m.CurrentUserIndex].Id == ClientManager.Instance.myUserInfo.Id).ToList();
            var theirMatches = matches.Where(m => m.CurrentState != Match.MatchState.Ended && m.Users[m.CurrentUserIndex].Id == ClientManager.Instance.myUserInfo.Id).ToList();
            var completedMatches = matches.Where(m => m.CurrentState == Match.MatchState.Ended).ToList();

            UpdateMatchSet(myMatchRows, myMatches);
            UpdateMatchSet(theirMatchRows, theirMatches);
            UpdateMatchSet(completedMatchRows, completedMatches);
        });
    }

    private void UpdateMatchSet(List<DashboardRow> rows, List<Match> matches)
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
            }
        }
    }
}