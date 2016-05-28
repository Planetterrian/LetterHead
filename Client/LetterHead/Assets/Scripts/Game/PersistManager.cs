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
        AudioListener.volume = SoundEnabled ? 1 : 0;
    }

    public bool ClearWord
    {
        get { return PlayerPrefs.GetInt("ClearWord", 0) == 1; }
        set { PlayerPrefs.SetInt("ClearWord", value ? 1 : 0); }
    }

    private void Start()
    {
        persistCanvas.gameObject.SetActive(true);
        OnMusicEnabledChanged();
        OnSoundEnabledChanged();
    }

    public void LoadMatch(int matchId, bool isDaily)
    {
        matchToLoadId = matchId;
        matchToLoadIsDaily = isDaily;

        SceneManager.LoadScene("game");
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("menu");
    }
}