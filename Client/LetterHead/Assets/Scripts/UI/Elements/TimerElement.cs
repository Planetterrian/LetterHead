using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using uTools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TimerElement : MonoBehaviour
{
    private float secondsRemaining;
    private TextMeshProUGUI label;
    private uTweenAlpha tweenAlpha;
    private bool active;

    public float flashUnderTime = -1;

    public UnityEvent OnTimeExpired;

    private void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();
        tweenAlpha = GetComponent<uTweenAlpha>();
        tweenAlpha.enabled = false;

        StartTimer(120);
        active = false;
    }

    private void Update()
    {
        if (active)
        {
            secondsRemaining -= Time.deltaTime;

            if(flashUnderTime != -1 && secondsRemaining < flashUnderTime && !tweenAlpha.enabled)
                tweenAlpha.Play();

            if (secondsRemaining <= 0)
            {
                active = false;
                secondsRemaining = 0;
                OnTimeExpired.Invoke();
                tweenAlpha.enabled = false;
                label.color = new Color(label.color.r, label.color.g, label.color.b, 1);
            }

            UpdateLabel();
        }
    }

    private void UpdateLabel()
    {
        var timespan = new TimeSpan(0, 0, Mathf.CeilToInt(secondsRemaining));
        label.text = ((int)timespan.TotalMinutes).ToString() + ":" + timespan.Seconds.ToString("00");
    }

    public void StartTimer(float time)
    {
        secondsRemaining = time;
        active = true;

        UpdateLabel();
    }

    public void SetTimer(int roundTimeSeconds)
    {
        secondsRemaining = roundTimeSeconds;
        UpdateLabel();
    }
}