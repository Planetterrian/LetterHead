using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuGui : Singleton<MenuGui>
{
    private GuiSceneManager sceneManager;
     
    public LoginScene loginScene;

    protected override void Awake()
    {
        base.Awake();

        sceneManager = GetComponent<GuiSceneManager>();
    }

    private void Start()
    {
        if (!string.IsNullOrEmpty(ClientManager.Instance.sessionId))
        {
            ClientManager.Instance.RefreshMyInfo((state) =>
            {
                if (state)
                {
                    if (string.IsNullOrEmpty(ClientManager.Instance.myUserInfo.Username))
                    {
                        LoadLogin();
                    }
                    else
                    {
                        LoadDashboard();
                    }
                }
                else
                {
                    LoadLogin();
                }
            });
        }
    }

    private void LoadLogin()
    {
        sceneManager.SetGuiScene("Login Scene");
    }

    private void LoadDashboard()
    {
        sceneManager.SetGuiScene("Dashboard Scene");
    }
}