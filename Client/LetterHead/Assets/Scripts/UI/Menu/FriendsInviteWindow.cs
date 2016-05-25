using System.Collections.Generic;
using LetterHeadShared.DTO;
using Newtonsoft.Json;
using UnityEngine;

public class FriendsInviteWindow : WindowController
{
    public GameObject friendsRowPrefab;

    public Transform gridParent;

    void OnWindowShown()
    {
        gridParent.DeleteChildren();

        Srv.Instance.POST("User/Friends", null, s =>
        {
            var friends = JsonConvert.DeserializeObject<List<UserInfo>>(s);

            foreach (var friend in friends)
            {
                var rowGo = GameObject.Instantiate(friendsRowPrefab);

            }
            
        });
    }
}
