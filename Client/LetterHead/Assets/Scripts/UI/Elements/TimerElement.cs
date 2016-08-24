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
    private AudioSource audio;

    private bool warning30Played;
    private bool warning20Played;
    private bool warning10Played;

    public float flashUnderTime = -1;
    public float soundUnderTime = -1;

    public uTweenAlpha flashLightTween;

    public UnityEvent OnTimeExpired;

    private void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();
        tweenAlpha = GetComponent<uTweenAlpha>();
        audio = GetComponent<AudioSource>();
        tweenAlpha.enabled = false;

        StartTimer(120);
        active = false;
    }

    private void Update()
    {
        if (active)
        {
            secondsRemaining -= Time.deltaTime;

            if (flashUnderTime != -1 && secondsRemaining < flashUnderTime && !tweenAlpha.enabled)
            {
                flashLightTween.Play();
                tweenAlpha.Play();
            }

            if (!SoundManager.Instance.Muted())
            {
                if (secondsRemaining <= 30 && !warning30Played)
                {
                    warning30Played = true;
                    audio.Play();
                }
                if (secondsRemaining <= 20 && !warning20Played)
                {
                    warning20Played = true;
                    audio.Play();
                }
                if (secondsRemaining <= 10 && !warning10Played)
                {
                    warning10Played = true;
                    audio.Play();
                }
            }

            if (secondsRemaining <= 0)
            {
                active = false;
                audio.Stop();
                secondsRemaining = 0;
                OnTimeExpired.Invoke();
                tweenAlpha.enabled = false;
                label.color = new Color(label.color.r, label.color.g, label.color.b, 1);
            }

            UpdateLabel();
        }
    }

    public void AddTime(float seconds)
    {
        secondsRemaining += seconds;
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        var timespan = new TimeSpan(0, 0, Mathf.CeilToInt(secondsRemaining));
        label.text = ((int)timespan.TotalMinutes).ToString() + ":" + timespan.Seconds.ToString("00");
    }

    public void StartTimer(float time)
    {
        tweenAlpha.ResetToInitialState();
        tweenAlpha.enabled = false;
        audio.Stop();

        warning30Played = false;
        warning20Played = false;
        warning10Played = false;

        secondsRemaining = time;
        active = true;

        UpdateLabel();
    }

    public void SetTimer(int roundTimeSeconds)
    {
        secondsRemaining = roundTimeSeconds;

        warning30Played = false;
        warning20Played = false;
        warning10Played = false;

        UpdateLabel();
    }

    public void Stop()
    {
        tweenAlpha.ResetToInitialState();
        flashLightTween.ResetToInitialState();

        flashLightTween.enabled = false;
        tweenAlpha.enabled = false;

        audio.Stop();
        active = false;
    }
}