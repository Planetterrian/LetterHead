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

    public RectTransform adPlaceholder;
    public RectTransform viewport;

    protected override void Awake()
    {
        base.Awake();

        sceneManager = GetComponent<GuiSceneManager>();
    }

    private void Start()
    {
        MusicManager.Instance.PlayMusic(menuMusic);

        AdManager.Instance.OnBannerAdShown.AddListener(OnBannerShown);
        AdManager.Instance.OnBannerAdHidden.AddListener(OnBannerHidden);

        if (AdManager.Instance.BannerShown())
        {
            OnBannerShown();
        }

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

    private void OnBannerHidden()
    {
        viewport.SetSize(new Vector2(viewport.GetSize().x, viewport.GetSize().y + adPlaceholder.GetSize().y));
        viewport.anchoredPosition = new Vector2(viewport.anchoredPosition.x, viewport.anchoredPosition.y - (adPlaceholder.GetSize().y / 2));
        adPlaceholder.gameObject.SetActive(false);
    }

    private void OnBannerShown()
    {
        adPlaceholder.gameObject.SetActive(true);
        viewport.SetSize(new Vector2(viewport.GetSize().x, viewport.GetSize().y - adPlaceholder.GetSize().y));
        viewport.anchoredPosition = new Vector2(viewport.anchoredPosition.x, viewport.anchoredPosition.y + (adPlaceholder.GetSize().y / 2));
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