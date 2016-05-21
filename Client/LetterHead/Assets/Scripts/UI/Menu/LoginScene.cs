using System;
using System.Collections.Generic;
using System.Linq;
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

    }

}