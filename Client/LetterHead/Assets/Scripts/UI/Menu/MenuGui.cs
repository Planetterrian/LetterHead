using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuGui : Singleton<MenuGui>
{
    private GuiSceneManager sceneManager;
     
    public LoginScene loginScene;
    public DashboardScene dashboardScene;

    public AudioClip menuMusic;

    protected override void Awake()
    {
        base.Awake();

        sceneManager = GetComponent<GuiSceneManager>();
    }

    private void Start()
    {
        MusicManager.Instance.PlayMusic(menuMusic);

        if (!string.IsNullOrEmpty(ClientManager.Instance.SessionId))
        {
            ClientManager.Instance.RefreshMyInfo(true, (state) =>
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
        else
        {
            LoadLogin();
        }
    }

    public void LoadLogin()
    {
        sceneManager.SetGuiScene(loginScene);
    }

    public void LoadDashboard()
    {
        sceneManager.SetGuiScene(dashboardScene);
    }
}