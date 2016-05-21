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

    private void Start()
    {
        SessionId = PlayerPrefs.GetString("sessId", "");

        if (!string.IsNullOrEmpty(SessionId))
            RefreshMyInfo();
    }

    public void RefreshMyInfo()
    {
        Srv.Instance.POST("User/MyInfo", null, s =>
        {
            myUserInfo = JsonConvert.DeserializeObject<UserInfo>(s);
        });
    }

    public void SetSessionId(string sessId)
    {
        SessionId = sessId;
        PlayerPrefs.SetString("sessId", sessId);
    }
}