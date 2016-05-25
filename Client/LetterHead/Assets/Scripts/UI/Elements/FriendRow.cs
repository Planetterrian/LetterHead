using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LetterHeadShared.DTO;
using UnityEngine;
using UnityEngine.UI;

public class FriendRow : MonoBehaviour
{
    public AvatarBox avatarBox;
    public UserInfo user;

    public void Set(UserInfo user, FriendsInviteWindow friendsInviteWindow)
    {
        this.user = user;

        avatarBox.SetAvatarImage(user.AvatarUrl);
        avatarBox.SetName(user.Username);

        GetComponent<Button>().onClick.AddListener(() => friendsInviteWindow.OnRowClicked(this));
    }
}
