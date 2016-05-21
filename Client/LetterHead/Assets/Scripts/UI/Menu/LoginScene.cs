using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoginScene : GuiScene
{
    public WindowController loginWindow;

    public void OnLoginEmailClicked()
    {
        loginWindow.ShowModal();
    }

    public void OnLoginFacebookClicked()
    {

    }

}