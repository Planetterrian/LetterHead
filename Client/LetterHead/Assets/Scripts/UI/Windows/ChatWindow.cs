using System;
using System.Collections;
using System.Collections.Generic;
using LetterHeadShared.DTO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatWindow : MonoBehaviour
{
    public GameObject chatRowPrefab;
    public GameObject chatRowPrefabMe;
    public Transform chatRowsParent;
    public TMP_InputField inputBox;
    public Button submitButton;
    public ScrollRect scroll;
    public TextMeshProUGUI title;

    private float lastPoll;
    private bool isPolling;
    private int lastRecievedMessageId;

    private void Start()
    {
        Poll(true);
        title.text = "Chatting with " + GameManager.Instance.OpponentUserName();
    }

    private void Poll(bool firstLoad = false)
    {
        if(isPolling)
            return;

        lastPoll = Time.time;
        isPolling = true;

        Srv.Instance.POST("Chat/GetAllMessages",
             new Dictionary<string, string>() { { "fromUserId", GameManager.Instance.OpponentUserId().ToString() }, { "lastRecievedMessageId", lastRecievedMessageId.ToString() }},
             s =>
             {
                 var messages = JsonConvert.DeserializeObject<List<ChatMessage>>(s);
                 foreach (var chatMessage in messages)
                 {
                     AddMessage(chatMessage);
                     lastRecievedMessageId = chatMessage.Id;

                     if (!firstLoad && chatMessage.SenderName != ClientManager.Instance.myUserInfo.Username)
                     {
                         GetComponent<AudioSource>().Play();
                     }
                 }

                 isPolling = false;
             }, s =>
             {
                 isPolling = false;
             });
    }

    void Update()
    {
        if (Time.time - lastPoll >= 4)
            Poll();
    }

    public void MessageSubmit()
    {
        if(!submitButton.interactable)
            return;

        var message = inputBox.text;
        if (message.Length == 0)
        {
            return;
        }

        submitButton.interactable = false;
        inputBox.interactable = false;

        Srv.Instance.POST("Chat/SendMessage",
            new Dictionary<string, string>()
            {
                {"toUserId", GameManager.Instance.OpponentUserId().ToString()},
                {"content", message}
            },
            s =>
            {
                inputBox.text = "";
                submitButton.interactable = true;
                inputBox.interactable = true;
                Poll();
            }, s =>
            {
                submitButton.interactable = true;
                inputBox.interactable = true;
                DialogWindowTM.Instance.Error(s);
            });
    }

    public void AddMessage(ChatMessage message)
    {
        GameObject rowGo;

        string imageUrl;
        if (message.SenderName == ClientManager.Instance.myUserInfo.Username)
        {
            imageUrl = ClientManager.Instance.myUserInfo.AvatarUrl;
            rowGo = GameObject.Instantiate(chatRowPrefabMe);
        }
        else
        {
            imageUrl = GameManager.Instance.OpponentAvatarUrl();
            rowGo = GameObject.Instantiate(chatRowPrefab);
        }

        var row = rowGo.GetComponent<ChatRow>();
        row.transform.SetParent(chatRowsParent);
        row.transform.ResetToOrigin();


        row.Set(message.SenderName, imageUrl, message.Message, (DateTime.UtcNow - TimeSpan.FromSeconds(message.SentAgo)));

        StartCoroutine(ScrollToBottom());
    }


    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        scroll.verticalNormalizedPosition = 0;
    }

}