using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatRow : MonoBehaviour
{
    public LayoutElement layout;
    public TextMeshProUGUI message;
    public TextMeshProUGUI timeAgoText;
    public AvatarBox avatarBox;

    private DateTime sentOn;
    private float lastTimeAgoUpdate;

    private void Start()
    {
    }

    public void Set(string sender, string senderImage, string content, DateTime sentOn)
    {
        this.sentOn = sentOn;
        message.text = content;

        timeAgoText.text = DateTimeHelper.GetFriendlyRelativeTimeShort(sentOn);
        lastTimeAgoUpdate = Time.time;

        if(avatarBox)
            avatarBox.SetAvatarImage(senderImage);

        layout.preferredHeight = message.preferredHeight + 22.324f;
        if (layout.preferredHeight < 55)
            layout.preferredHeight = 55;
    }

    void Update()
    {


        if (Time.time - lastTimeAgoUpdate > 30)
        {
            timeAgoText.text = DateTimeHelper.GetFriendlyRelativeTimeShort(sentOn);
            lastTimeAgoUpdate = Time.time;
        }
    }
     
}