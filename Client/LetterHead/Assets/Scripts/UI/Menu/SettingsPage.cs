﻿using System;
using System.Collections.Generic;
using System.Linq;
using Facebook.Unity;
using Newtonsoft.Json;
using TMPro;
using UI.Pagination;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPage : Page
{
    public Toggle musicEnabledToggle;
    public Toggle soundEnabledToggle;
    public Toggle clearWordToggle;
    public Toggle notificationsToggle;

    public GameObject linkFacebookButton;
    public GameObject unLinkFacebookButton;

    public Button premiumUpgradeButton;

    private void Awake()
    {
    }

    private void Start()
    {
        IapManager.Instance.OnItemsUpdated.AddListener(Refresh);
    }

    public void Refresh()
    {
        
        linkFacebookButton.SetActive(string.IsNullOrEmpty(ClientManager.Instance.myUserInfo.FacebookPictureUrl));
        unLinkFacebookButton.SetActive(!string.IsNullOrEmpty(ClientManager.Instance.myUserInfo.FacebookPictureUrl) && ClientManager.Instance.myUserInfo.HasEmail);

        premiumUpgradeButton.interactable = AdManager.Instance.AdsEnabled();

        musicEnabledToggle.isOn = PersistManager.Instance.MusicEnabled;
        clearWordToggle.isOn = PersistManager.Instance.ClearWord;
        soundEnabledToggle.isOn = PersistManager.Instance.SoundEnabled;
        notificationsToggle.isOn = PersistManager.Instance.NotificationsEnabled;
    }

    private string SettingsString()
    {
        return (PersistManager.Instance.SoundEnabled ? "1" : "0") + (PersistManager.Instance.MusicEnabled ? "1" : "0") + (PersistManager.Instance.ClearWord ? "1" : "0") + (PersistManager.Instance.NotificationsEnabled ? "1" : "0");
    }

    public void OnMusicToggleChanged()
    {
        PersistManager.Instance.MusicEnabled = musicEnabledToggle.isOn;
        Srv.Instance.POST("User/ChangeSettings", new Dictionary<string, string>() {{"settings", SettingsString()}}, s => { });
    }

    public void OnSoundToggleChanged()
    {
        PersistManager.Instance.SoundEnabled = soundEnabledToggle.isOn;
        Srv.Instance.POST("User/ChangeSettings", new Dictionary<string, string>() {{"settings", SettingsString()}}, s => { });
    }

    public void OnClearWordToggleChanged()
    {
        PersistManager.Instance.ClearWord = clearWordToggle.isOn;
        Srv.Instance.POST("User/ChangeSettings", new Dictionary<string, string>() {{"settings", SettingsString()}}, s => { });
    }

    public void OnNotificationsToggleChanged()
    {
        PersistManager.Instance.NotificationsEnabled = notificationsToggle.isOn;

        if (PersistManager.Instance.NotificationsEnabled)
        {
            NotificationManager.Instance.RegisterForNotifications();
        }
        else
        {
            NotificationManager.Instance.UnregisterForNotifications();
        }

        Srv.Instance.POST("User/ChangeSettings", new Dictionary<string, string>() { { "settings", SettingsString() } }, s => { });
    }

    public void UpgradeToPremiumClicked()
    {
        IapManager.Instance.RequestPurchase("com.we3workshop.lhead.premium3");
    }

    public void LogoutClicked()
    {
        ClientManager.Instance.Logout();
        MenuGui.Instance.LoadLogin();
    }

    public void DisconnectFacebookClicked()
    {
        DialogWindowTM.Instance.Show("Disconnect Facebook", "Please wait", () => { }, null, "", "");
        Srv.Instance.POST("User/FacebookDisconnect", null, s =>
        {
            ClientManager.Instance.myUserInfo.FacebookPictureUrl = "";
            ClientManager.Instance.myUserInfo.AvatarUrl = "sprite:Picture1";
            Refresh();
            DialogWindowTM.Instance.Show("Disconnect Facebook", "Your Facebook account has been disconnected from Letter Head.", () => { });
        }, DialogWindowTM.Instance.Error);
    }

    public void CconnectFacebookClicked()
    {
        FB.LogInWithReadPermissions(new List<string>() { "user_friends" }, FacebookLoginCallback);
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

        Srv.Instance.POST("User/FacebookConnect", new Dictionary<string, string>() { { "token", result.AccessToken.TokenString } }, (s) =>
        {
            AchievementManager.Instance.Set("facebook");

            ClientManager.Instance.RefreshMyInfo(false, b =>
            {
                DialogWindowTM.Instance.Hide();
                Refresh();
            });
        }, s =>
        {
            DialogWindowTM.Instance.Error(s);
        });
    }

    public void AboutClicked()
    {
        
    }

    public void HowToPlayClicked()
    {
        
    }

    public void RestorePurchasesClicked()
    {
        NPBinding.Billing.RestorePurchases();

    }
}