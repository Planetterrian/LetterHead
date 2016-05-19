using System;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared.DTO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

public class ClientManager : Singleton<ClientManager>
{
    public string sessionId;

    public UserInfo myUserInfo;

    public UnityEvent OnMyInfoUpdated;

    private void Start()
    {
        if(!string.IsNullOrEmpty(sessionId))
            RefreshMyInfo();
    }

    public void RefreshMyInfo()
    {
        Srv.Instance.POST("User/MyInfo", null, s =>
        {
            myUserInfo = JsonConvert.DeserializeObject<UserInfo>(s);
        });
    }
}