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

    public UnityEvent OnMyInfoUpdated;

    private string sessionId;
    public string SessionId
    {
        get { return sessionId; }
        private set { sessionId = value; }
    }

    protected override void Awake()
    {
        base.Awake();

        SessionId = PlayerPrefs.GetString("sessId", "");
    }

    private void Start()
    {
/*
        if (!string.IsNullOrEmpty(SessionId))
            RefreshMyInfo();*/
    }

    public void RefreshMyInfo(bool isFirstLoad, Action<bool> onInfoLoaded = null)
    {
        Srv.Instance.POST("User/MyInfo", new Dictionary<string, string>() { { "isFirstLoad", isFirstLoad ? "True" : "False"} }, s =>
        {
            myUserInfo = JsonConvert.DeserializeObject<UserInfo>(s);

            if (onInfoLoaded != null)
                onInfoLoaded(true);
        }, s =>
        {
            myUserInfo = null;

            if (onInfoLoaded != null)
                onInfoLoaded(false);
        });
    }

    public void SetSessionId(string sessId, bool isFirstLoad = false)
    {
        SessionId = sessId;
        PlayerPrefs.SetString("sessId", sessId);
        RefreshMyInfo(isFirstLoad);
    }

    public int UserId()
    {
        return myUserInfo.Id;
    }

    public void Logout()
    {
        PlayerPrefs.SetString("sessId", "");
        myUserInfo = null;
    }
}