using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UI.Pagination;
using UnityEngine;

public class NewGamePage : Page
{
    public WindowController friendsWindow;

    public void OnInviteFriendClicked()
    {
        friendsWindow.ShowModal();
    }

    public void OnRandomOpponentClicked()
    {
        DialogWindowTM.Instance.Show("New Game", "One moment...", () => { }, () => { }, "");

        Srv.Instance.POST("Match/Random", null, s =>
        {
            DialogWindowTM.Instance.Show("New Game", "We are searching for an opponent. You will receive a notification when one is found.", () => { });
        }, DialogWindowTM.Instance.Error );
    }

    public void DailyGameClicked()
    {
        Srv.Instance.POST("Match/RequestDailyGameStart", null, s =>
        {
            var matchId = JsonConvert.DeserializeObject<int>(s);
            AdManager.Instance.ShowInterstitial();
            PersistManager.Instance.LoadMatch(matchId, true);
        }, DialogWindowTM.Instance.Error);
    }

    public void SoloGameClicked()
    {
        Srv.Instance.POST("Match/RequestSoloGameStart", null, s =>
        {
            var matchId = JsonConvert.DeserializeObject<int>(s);
            AdManager.Instance.ShowInterstitial();
            PersistManager.Instance.LoadMatch(matchId, true);
        }, DialogWindowTM.Instance.Error);
    }

}