using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

class AvatarSelectWindow : WindowController
{
    public Transform gridParent;
    public ProfilePage profilePage;
    public GameObject avatarButtonPrefab;

    private bool initDone;

    void OnWindowShown()
    {
        if(initDone)
            return;

        initDone = true;

        foreach (var sprite in AvatarManager.Instance.avatars_sprites)
        {
            var but = GameObject.Instantiate(avatarButtonPrefab) as GameObject;
            but.transform.SetParent(gridParent);
            but.transform.ResetToOrigin();

            but.GetComponent<Image>().sprite = sprite;
            var sprite1 = sprite;

            but.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnAvatarClicked(sprite1.name);
            });
        }

    }

    private void OnAvatarClicked(string avatarName)
    {
        Hide();

        profilePage.OnAvatarChanged(avatarName);
    }
}
