using System;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared.DTO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

public class ClientManager : Singleton<ClientManager>
{
    public UserInfo myUserInfo;

    private int lastUserId;

    private string sessionId;
    public string SessionId
    {
        get { return sessionId; }
        private set { sessionId = value; }
    }

    public bool CanDoDaily { get; set; }
    public bool IsNewLogin { get; set; }

    protected override void Awake()
    {
        base.Awake();

        SessionId = PlayerPrefs.GetString("sessId", "");

/*

        SessionId = "f34b8e64-9457-4d2e-944b-7cf06d904836";
        Debug.LogError("USING MANUAL SESSID");
*/

    }

    private void Start()
    {

    }

    public bool PlayerDataLoaded()
    {
        return myUserInfo != null;
    }


    public int PowerupCount(LetterHeadShared.Powerup.Type powerupType)
    {
        if (myUserInfo == null)
            return 0;

        if ((int)powerupType >= myUserInfo.PowerupCountList.Count)
            return 0;

        return myUserInfo.PowerupCountList[(int)powerupType];
    }

    public void RefreshMyInfo(bool isFirstLoad, Action<bool> onInfoLoaded = null)
    {
        Srv.Instance.POST("User/MyInfo", new Dictionary<string, string>() { { "isFirstLoad", isFirstLoad ? "True" : "False"} }, s =>
        {
            myUserInfo = JsonConvert.DeserializeObject<UserInfo>(s);

            if (lastUserId != myUserInfo.Id)
            {
                IsNewLogin = true;
                lastUserId = myUserInfo.Id;
            }

            PersistManager.Instance.SoundEnabled = myUserInfo.Settings.Substring(0, 1) == "1";
            PersistManager.Instance.MusicEnabled = myUserInfo.Settings.Substring(1, 1) == "1";
            PersistManager.Instance.ClearWord = myUserInfo.Settings.Substring(2, 1) == "1";
            PersistManager.Instance.NotificationsEnabled = myUserInfo.Settings.Substring(3, 1) == "1";

            if (myUserInfo.IsPremium)
                AdManager.Instance.DisableAds();
            else if(AdManager.Instance.ShouldShowBanner())
            {
                AdManager.Instance.EnableAds();
            }

            if(PersistManager.Instance.NotificationsEnabled)
                NotificationManager.Instance.RegisterForNotifications();

            if (onInfoLoaded != null)
                onInfoLoaded(true);
        }, s =>
        {
            myUserInfo = null;

            if (onInfoLoaded != null)
                onInfoLoaded(false);
        });
    }

    public void SetSessionId(string sessId, bool isFirstLoad = false, Action<bool> onInfoLoaded = null)
    {
        SessionId = sessId;
        PlayerPrefs.SetString("sessId", sessId);
        RefreshMyInfo(isFirstLoad, onInfoLoaded);
    }

    public int UserId()
    {
        if (myUserInfo == null)
            return -1;

        return myUserInfo.Id;
    }

    public void Logout()
    {
        PlayerPrefs.SetString("sessId", "");
        myUserInfo = null;
    }
}