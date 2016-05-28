using System;
using System.Collections.Generic;
using System.Linq;
using UI.Pagination;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPage : Page
{
    public Toggle musicEnabledToggle;
    public Toggle soundEnabledToggle;
    public Toggle clearWordToggle;
    public Toggle notificationsToggle;

    private void Awake()
    {
    }

    private void Start()
    {
    }

    public void Refresh()
    {
        musicEnabledToggle.isOn = PersistManager.Instance.MusicEnabled;
        clearWordToggle.isOn = PersistManager.Instance.ClearWord;
        soundEnabledToggle.isOn = PersistManager.Instance.SoundEnabled;
        notificationsToggle.isOn = PersistManager.Instance.NotificationsEnabled;
    }

    public void OnMusicToggleChanged()
    {
        PersistManager.Instance.MusicEnabled = musicEnabledToggle.isOn;
    }

    public void OnSoundToggleChanged()
    {
        PersistManager.Instance.SoundEnabled = musicEnabledToggle.isOn;
    }

    public void OnClearWordToggleChanged()
    {
        PersistManager.Instance.ClearWord = clearWordToggle.isOn;
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
    }

    public void UpgradeToPremiumClicked()
    {

    }

    public void LogoutClicked()
    {
        ClientManager.Instance.Logout();
        MenuGui.Instance.LoadLogin();
    }

    public void DisconnectFacebookClicked()
    {
        
    }

    public void AboutClicked()
    {
        
    }

    public void HowToPlayClicked()
    {
        
    }
}