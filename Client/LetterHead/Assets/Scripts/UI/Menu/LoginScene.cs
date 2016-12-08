using System;
using System.Collections.Generic;
using System.Linq;
using Facebook.Unity;
using Newtonsoft.Json;
using UnityEngine;

public class LoginScene : GuiScene
{
    public WindowController loginWindow;
    public WindowController registerDetailsWindow;

    public override void OnBeginShow()
    {
        base.OnBeginShow();

        if (ClientManager.Instance.myUserInfo != null && string.IsNullOrEmpty(ClientManager.Instance.myUserInfo.Username))
        {
            registerDetailsWindow.ShowModal();
        }
    }

    public void OnLoginEmailClicked()
    {
        loginWindow.ShowModal();
    }

    public void OnLoginFacebookClicked()
    {
        FB.LogInWithReadPermissions(new List<string>() { "user_friends" }, FacebookLoginCallback);

    }

    public class LoginResult
    {
        public string SessionId;
        public int MatchCount;
    }

    private void FacebookLoginCallback(ILoginResult result)
    {
        if (result.Cancelled)
        {
            DialogWindowTM.Instance.Hide();
            return;
        }

        if (!string.IsNullOrEmpty(result.Error))
        {
            DialogWindowTM.Instance.Error(result.Error);
            return;
        }

        DialogWindowTM.Instance.Show("Facebook", "We are processing your login... Please wait.", () => { }, () => { }, "");

        Srv.Instance.POST("User/FacebookLogin", new Dictionary<string, string>() {{"token", result.AccessToken.TokenString}, { "version", "2"} }, (s) =>
        {
            var facebookLoginResult = JsonConvert.DeserializeObject<LoginResult>(s);
            AchievementManager.Instance.Set("facebook");
            ClientManager.Instance.SetSessionId(facebookLoginResult.SessionId, true, b =>
            {
                MenuGui.Instance.LoadDashboard(facebookLoginResult.MatchCount == 0 ? PersistManager.NewGamePage : PersistManager.DashboardPage);
                DialogWindowTM.Instance.Hide();
            });
        }, s =>
        {
            DialogWindowTM.Instance.Error(s);
        });
        //Debug.Log(result.AccessToken.TokenString);
    }
}