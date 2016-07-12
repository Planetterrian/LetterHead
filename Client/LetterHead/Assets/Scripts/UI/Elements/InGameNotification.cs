using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using uTools;
using UnityEngine;

public class InGameNotification : Singleton<InGameNotification>
{
    public TextMeshProUGUI label;
    public uTweenTransform tween;

    private TimerManager.TimerEvent hideEvent;

    public void Show(string message)
    {
        label.text = message;
        tween.Play();

        TimerManager.Instance.CancelEvent(hideEvent);
        hideEvent = TimerManager.AddEvent(3f, Hide);
    }

    private void Hide()
    {
        tween.Play(PlayDirection.Reverse);
    }
}