using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistManager : Singleton<PersistManager>
{
    public Canvas persistCanvas;

    public int matchToLoadId;
    public bool matchToLoadIsDaily;

    [HideInInspector]
    public int initialDashPage = 1;

    public const int DashboardPage = 1;
    public const int NewGamePage = 2;

    public void SetInitialDashPage(int page)
    {
        initialDashPage = page;
    }

    public bool MusicEnabled
    {
        get { return PlayerPrefs.GetInt("MusicEnabled", 1) == 1; }
        set
        {
            PlayerPrefs.SetInt("MusicEnabled", value ? 1 : 0);
            OnMusicEnabledChanged();
        }
    }

    public bool NotificationsEnabled
    {
        get { return PlayerPrefs.GetInt("NotificationsEnabled", 1) == 1; }
        set
        {
            PlayerPrefs.SetInt("NotificationsEnabled", value ? 1 : 0);
        }
    }

    private void OnMusicEnabledChanged()
    {
        MusicManager.Instance.SetMusicVolume(MusicEnabled ? MusicManager.Instance.volume : 0);
    }

    public bool SoundEnabled
    {
        get { return PlayerPrefs.GetInt("SoundEnabled", 1) == 1; }
        set
        {
            PlayerPrefs.SetInt("SoundEnabled", value ? 1 : 0);
            OnSoundEnabledChanged();
        }
    }

    private void OnSoundEnabledChanged()
    {
    }

    public bool ClearWord
    {
        get { return PlayerPrefs.GetInt("ClearWord", 1) == 1; }
        set { PlayerPrefs.SetInt("ClearWord", value ? 1 : 0); }
    }

    private void Start()
    {
        persistCanvas.gameObject.SetActive(true);
        OnMusicEnabledChanged();
        OnSoundEnabledChanged();
/*

        if (Application.platform == RuntimePlatform.Android)
        {
            Application.targetFrameRate = 30;
            Debug.Log("FPS set to 30");
        }*/
    }

    public void LoadMatch(int matchId, bool isDaily)
    {
        matchToLoadId = matchId;
        matchToLoadIsDaily = isDaily;

        DialogWindowTM.Instance.Show("", "Loading... please wait.", () => { }, () => { }, "");

        SceneManager.LoadScene("game");
    }

    public void LoadMenu(int _initialDashPage = 1)
    {
        initialDashPage = _initialDashPage;
        TutorialManager.Instance.HideTutorial();
        SceneManager.LoadScene("menu");
    }
}