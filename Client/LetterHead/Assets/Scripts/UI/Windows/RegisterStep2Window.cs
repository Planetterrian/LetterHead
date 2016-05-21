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
    public Dropdown avatarDropdown;

    void Start()
    {
        avatarDropdown.ClearOptions();
        
        foreach (var sprite in AvatarManager.Instance.avatars_sprites)
        {
            avatarDropdown.options.Add(new Dropdown.OptionData()
                                       {
                                           image = sprite,
                                           text = sprite.name
            });
        }

        avatarDropdown.value = -1;

        avatarDropdown.value = UnityEngine.Random.Range(0, avatarDropdown.options.Count);
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
                                                      {"Username", usernameInput.text}, {"Avatar", avatarDropdown.options[avatarDropdown.value].text}
                                                  }, s =>
                                                  {

                                                  }, s =>
                                                  {
                                                    submitButton.interactable = true;
                                                  });
    }
}