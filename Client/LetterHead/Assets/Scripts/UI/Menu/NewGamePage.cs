using System;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared;
using Newtonsoft.Json;
using UI.Pagination;
using UnityEngine;
using UnityEngine.UI;

public class NewGamePage : Page
{
    public WindowController friendsWindow;
    public InviteUsernameWindow inviteUsernameWindow;

    public Button dailyGameButton;
    public Button soloGameButton;
    public Button versusGameButton;
    public Button friendsGameButton;
    public Button leaderbaordGameButton;

    public Toggle normalGameToggle;
    public Toggle advancedGameToggle;

    public static NewGamePage Instance;

    public override void OnShow()
    {
        base.OnShow();

        if (ScoringType() == CategoryManager.Type.Normal || ScoringType() == CategoryManager.Type.NormalRetro)
            normalGameToggle.isOn = true;
        else if (ScoringType() == CategoryManager.Type.Advanced || ScoringType() == CategoryManager.Type.AdvancedRetro)
            advancedGameToggle.isOn = true;

        Refresh();
    }

    private void OnApplicationFocus(bool state)
    {
        if (state)
        {
            Srv.Instance.POST("Match/List", new Dictionary<string, string>() { { "scoringType", ScoringType().ToString() } }, s =>
            {
                var list = JsonConvert.DeserializeObject<HomePage.ListInfo>(s);
                ClientManager.Instance.CanDoDaily = list.CanDoDailyArray;
                Refresh();
            });
        }
    }

    public static CategoryManager.Type ScoringType()
    {
        var scoringType = (CategoryManager.Type)PlayerPrefs.GetInt("DefaultScoringType" + ClientManager.Instance.UserId(), (int)CategoryManager.Type.NormalRetro);

        // Convert to retro mode
        if (scoringType == CategoryManager.Type.Normal)
            scoringType = CategoryManager.Type.NormalRetro;
        else if (scoringType == CategoryManager.Type.Advanced)
            scoringType = CategoryManager.Type.AdvancedRetro;

        return scoringType;
    }

    public void OnScoringTypeChanged()
    {
        if (normalGameToggle.isOn)
            PlayerPrefs.SetInt("DefaultScoringType" + ClientManager.Instance.UserId(), (int)CategoryManager.Type.NormalRetro);
        else if (advancedGameToggle.isOn)
            PlayerPrefs.SetInt("DefaultScoringType" + ClientManager.Instance.UserId(), (int)CategoryManager.Type.AdvancedRetro);

        Refresh();
    }

    public void Refresh()
    {
        dailyGameButton.interactable = ClientManager.Instance.CanDoDaily[(int)ScoringType()] && !TutorialManager.Instance.ShouldDisableNewGameButtons();
    }

    void Awake()
    {
        Instance = this;
    }

    public void OnInviteFriendClicked()
    {
        friendsWindow.ShowModal();
    }

    public void OnInviteUsernameClicked()
    {
        inviteUsernameWindow.ShowModal();
    }

    public void OnRandomOpponentClicked()
    {
        DialogWindowTM.Instance.Show("New Game", "One moment...", () => { }, () => { }, "");

        Srv.Instance.POST("Match/Random", new Dictionary<string, string>() { { "scoringType", ScoringType().ToString() }}, s =>
        {
            DialogWindowTM.Instance.Show("New Game", "We are searching for an opponent. You will receive a notification when one is found.", () => { });
        }, DialogWindowTM.Instance.Error );
    }

    public void DailyGameClicked()
    {
        DialogWindowTM.Instance.Show("New Game", "One moment...", () => { }, () => { }, "");

        Srv.Instance.POST("Match/RequestDailyGameStart", new Dictionary<string, string>() { { "scoringType", ScoringType().ToString() } }, s =>
        {
            var matchId = JsonConvert.DeserializeObject<int>(s);
            PersistManager.Instance.LoadMatch(matchId, true);
        }, DialogWindowTM.Instance.Error);
    }

    public void SoloGameClicked()
    {
        DialogWindowTM.Instance.Show("New Game", "One moment...", () => { }, () => { }, "");

        TutorialManager.Instance.OnSoloGameClicked();

        Srv.Instance.POST("Match/RequestSoloGameStart", new Dictionary<string, string>() { { "scoringType", ScoringType().ToString() } }, s =>
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