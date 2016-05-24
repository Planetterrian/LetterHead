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
        FB.LogInWithReadPermissions(new List<string>() {"friends"}, FacebookLoginCallback);
    }

    private void FacebookLoginCallback(ILoginResult result)
    {
        if (!string.IsNullOrEmpty(result.Error))
        {
            DialogWindowTM.Instance.Error(result.Error);
            return;
        }

        Srv.Instance.POST("User/FacebookLogin", new Dictionary<string, string>() {{"token", result.AccessToken.TokenString}}, (s) =>
        {
            var sessionId = JsonConvert.DeserializeObject<string>(s);
            ClientManager.Instance.SetSessionId(sessionId);
            MenuGui.Instance.LoadDashboard();
        });
        Debug.Log(result.AccessToken.TokenString);
    }
}