using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UI.Pagination;
using UnityEngine;
using UnityEngine.UI;

public class NewGamePage : Page
{
    public WindowController friendsWindow;
    public Button dailyGameButton;

    public static NewGamePage Instance;

    public override void OnShow()
    {
        base.OnShow();

        Refresh();
    }

    public void Refresh()
    {
        dailyGameButton.interactable = ClientManager.Instance.CanDoDaily;
    }

    void Awake()
    {
        Instance = this;
    }

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
        DialogWindowTM.Instance.Show("New Game", "One moment...", () => { }, () => { }, "");

        Srv.Instance.POST("Match/RequestDailyGameStart", null, s =>
        {
            var matchId = JsonConvert.DeserializeObject<int>(s);
            PersistManager.Instance.LoadMatch(matchId, true);
        }, DialogWindowTM.Instance.Error);
    }

    public void SoloGameClicked()
    {
        DialogWindowTM.Instance.Show("New Game", "One moment...", () => { }, () => { }, "");

        Srv.Instance.POST("Match/RequestSoloGameStart", null, s =>
        {
            var matchId = JsonConvert.DeserializeObject<int>(s);
            PersistManager.Instance.LoadMatch(matchId, true);
        }, DialogWindowTM.Instance.Error);
    }

}