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
    public Button soloGameButton;
    public Button versusGameButton;
    public Button friendsGameButton;
    public Button leaderbaordGameButton;

    public static NewGamePage Instance;

    public override void OnShow()
    {
        base.OnShow();

        Refresh();
    }

    private void OnApplicationFocus(bool state)
    {
        print(state);
        if (state)
        {
            Srv.Instance.POST("Match/List", null, s =>
            {
                var list = JsonConvert.DeserializeObject<HomePage.ListInfo>(s);
                ClientManager.Instance.CanDoDaily = list.CanDoDaily;
                Refresh();
            });
        }
    }


    public void Refresh()
    {
        dailyGameButton.interactable = ClientManager.Instance.CanDoDaily && !TutorialManager.Instance.ShouldDisableNewGameButtons();
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

        TutorialManager.Instance.OnSoloGameClicked();

        Srv.Instance.POST("Match/RequestSoloGameStart", null, s =>
        {
            var matchId = JsonConvert.DeserializeObject<int>(s);
            PersistManager.Instance.LoadMatch(matchId, true);
        }, DialogWindowTM.Instance.Error);
    }

    public void EnableOnlySoloGame()
    {
        dailyGameButton.interactable = false;
        friendsGameButton.interactable = false;
        leaderbaordGameButton.interactable = false;
        soloGameButton.interactable = true;
        versusGameButton.interactable = false;
    }
}