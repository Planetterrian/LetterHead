using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmailLoginWindow : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    public Button loginButton;
    public Button registerButton;

    void OnEnable()
    {
        loginButton.interactable = true;
        registerButton.interactable = true;

        passwordInput.text = "";
    }
    
    public void OnLoginClicked()
    {
        var email = emailInput.text;
        var password = passwordInput.text;

        loginButton.interactable = false;
        registerButton.interactable = false;

        Srv.Instance.POST("User/LoginEmail", new Dictionary<string, string>()
                                        {
                                            {"Email", email}, {"Password", password}
                                        }, s =>
                                        {
                                            var sessionId = JsonConvert.DeserializeObject<string>(s);
                                            ClientManager.Instance.SetSessionId(sessionId);
                                            GetComponent<WindowController>().Hide();
                                            MenuGui.Instance.LoadDashboard();
                                        }, s =>
                                        {
                                            DialogWindowTM.Instance.Show("Error", s, () => { });

                                            loginButton.interactable = true;
                                            registerButton.interactable = true;
                                        });
    }

    public void OnRegisterClicked()
    {
        var email = emailInput.text;
        var password = passwordInput.text;

        loginButton.interactable = false;
        registerButton.interactable = false;

        Srv.Instance.POST("User/RegisterEmail", new Dictionary<string, string>()
                                        {
                                            {"Email", email}, {"Password", password}
                                        }, s =>
                                        {
                                            var sessionId = JsonConvert.DeserializeObject<string>(s);
                                            GetComponent<WindowController>().Hide();
                                            ClientManager.Instance.SetSessionId(sessionId);
                                            MenuGui.Instance.loginScene.registerDetailsWindow.ShowModal();
                                        }, s =>
                                        {
                                            DialogWindowTM.Instance.Show("Error", s, () => { });

                                            loginButton.interactable = true;
                                            registerButton.interactable = true;
                                        });
    }
}