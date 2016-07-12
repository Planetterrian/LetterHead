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

    public LoadingEffect loadingEffect;

    private static bool firstLoad = true;
    private bool bannerShown;

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
            if (firstLoad)
            {
                firstLoad = false;
                sceneManager.SetGuiScene("Loggin In Scene");
            }
            else
            {
                loadingEffect.loading = true;
            }

            ClientManager.Instance.RefreshMyInfo(true, (state) =>
            {
                loadingEffect.loading = false;
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
        if(!bannerShown)
            return;

        bannerShown = false;
        viewport.SetSize(new Vector2(viewport.GetSize().x, viewport.GetSize().y + adPlaceholder.GetSize().y));
        viewport.anchoredPosition = new Vector2(viewport.anchoredPosition.x, viewport.anchoredPosition.y - (adPlaceholder.GetSize().y / 2));
        adPlaceholder.gameObject.SetActive(false);
    }

    private void OnBannerShown()
    {
        if (bannerShown)
            return;

        bannerShown = true;
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

    public void OpenLink(string url)
    {
        Application.OpenURL(url);
    }
}