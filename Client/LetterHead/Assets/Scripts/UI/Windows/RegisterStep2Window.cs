using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegisterStep2Window : MonoBehaviour
{
    public Button submitButton;
    public TMP_InputField usernameInput;
    public AvatarBox avatarBox;
    public AvatarSelectWindow avatarSelectWindow;

    private string avatarName;

    void Start()
    {
        avatarName = AvatarManager.Instance.avatars_sprites[UnityEngine.Random.Range(0, AvatarManager.Instance.avatars_sprites.Length)].name;
        avatarBox.SetAvatarImage("sprite:" + avatarName);
    }

    public void ShowAvatarSelect()
    {
        avatarSelectWindow.OnSelected = (s) =>
        {
            avatarName = s;
            avatarBox.SetAvatarImage("sprite:" + avatarName);
        };

        avatarSelectWindow.ShowModal();
    }

    void OnEnable()
    {
        submitButton.interactable = true;
    }

    public void OnSubmit()
    {
        submitButton.interactable = false;
        Srv.Instance.POST("User/RegisterDetails", new Dictionary<string, string>()
                                                  {
                                                      {"Username", usernameInput.text}, {"Avatar", avatarName}
                                                  }, s =>
                                                  {
                                                      if (s == "1")
                                                      {
                                                          GetComponent<WindowController>().Hide();
                                                          MenuGui.Instance.LoadDashboard();
                                                          ClientManager.Instance.RefreshMyInfo(false);
                                                      }
                                                  }, s =>
                                                  {
                                                      submitButton.interactable = true;
                                                      DialogWindowTM.Instance.Show("Error", s, () => { });
                                                  });
    }
}