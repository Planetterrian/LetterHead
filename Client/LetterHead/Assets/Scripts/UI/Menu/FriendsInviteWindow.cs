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
                rowGo.transform.SetParent(gridParent);
                rowGo.transform.ResetToOrigin();

                var row = rowGo.GetComponent<FriendRow>();
                row.Set(friend, this);
            }
        });
    }

    public void OnRowClicked(FriendRow friendRow)
    {
        DialogWindowTM.Instance.Show("Invite", "Sending invite to " + friendRow.user.Username, () => { }, () => { }, "");
        Srv.Instance.POST("Match/Invite", new Dictionary<string, string>() {{"userId", friendRow.user.Id.ToString()}},
            (s) =>
            {
                DialogWindowTM.Instance.Show("Invite", "Invite sent!", () => { });
            }, DialogWindowTM.Instance.Error);
    }
}
