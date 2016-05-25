using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LetterHeadShared.DTO;
using UnityEngine;

class FriendRow : MonoBehaviour
{
    public AvatarBox avatarBox;

    public void OnClicked()
    {
        
    }

    public void Set(UserInfo user)
    {
        avatarBox.SetAvatarImage(user.AvatarUrl);
        avatarBox.SetName(user.Username);
    }
}
