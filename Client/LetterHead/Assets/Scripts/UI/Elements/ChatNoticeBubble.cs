using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using uTools;
using UnityEngine;

public class ChatNoticeBubble : MonoBehaviour
{
    public float pollRate;
    public WindowController chatWindow;
    public TextMeshProUGUI countLabel;
    public uTweener tween;

    private float lastPoll;
    private int lastMessageCount;

    private void Start()
    {
    }

    void Update()
    {
        if (Time.time - lastPoll >= pollRate)
            Poll();
    }

    private void Poll()
    {
        lastPoll = Time.time;

        if(chatWindow.IsShown())
            return;

        if(GameManager.Instance.MatchDetails != null)
        {
            Srv.Instance.POST("Chat/UnreadMessageCount",
                new Dictionary<string, string>() {{"fromUserId", GameManager.Instance.OpponentUserId().ToString()}},
                s =>
                {
                    var count = JsonConvert.DeserializeObject<int>(s);
                    if (count == 0)
                    {
                        countLabel.text = "";
                    }
                    else
                    {
                        if (count > lastMessageCount)
                        {
                            tween.Play();
                        }

                        lastMessageCount = count;
                        countLabel.text = count.ToString();
                    }
                });
        }
    }

    public void ShowChatWindow()
    {
        lastMessageCount = 0;
        countLabel.text = "";
        chatWindow.ShowModal();
    }
}