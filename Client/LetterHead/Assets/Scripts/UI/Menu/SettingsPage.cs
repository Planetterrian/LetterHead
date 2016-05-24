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