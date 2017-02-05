using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InviteUsernameWindow : WindowController
{
    public TMP_InputField usernameInput;
    public Button submitButton;

    private void Start()
    {
    }

    public void Submit()
    {
        submitButton.interactable = false;
        usernameInput.interactable = false;

        var username = usernameInput.text;

        DialogWindowTM.Instance.Show("Invite", "Sending invite to " + username + ".", () => { }, () => { }, "");

        Srv.Instance.POST("Match/InviteByUsername", new Dictionary<string, string>() {{"username", username }, {"scoringType", NewGamePage.ScoringType().ToString()} }, s =>
        {
            DialogWindowTM.Instance.Show("Invite", "Invite sent to " + username + ".", () => { });
            submitButton.interactable = true;
            usernameInput.interactable = true;
        }, s =>
        {
            DialogWindowTM.Instance.Error(s);
            submitButton.interactable = true;
            usernameInput.interactable = true;
        });
    }

    void OnWindowShown()
    {
        submitButton.interactable = true;
        usernameInput.interactable = true;
    }
}