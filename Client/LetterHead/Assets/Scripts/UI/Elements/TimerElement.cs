using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TimerElement : MonoBehaviour
{
    private float secondsRemaining;
    private TextMeshProUGUI label;

    private bool active;

    public UnityEvent OnTimeExpired;

    private void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();

        StartTimer(120);
        active = false;
    }

    private void Update()
    {
        if (active)
        {
            secondsRemaining -= Time.deltaTime;

            if (secondsRemaining <= 0)
            {
                active = false;
                secondsRemaining = 0;
                OnTimeExpired.Invoke();
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

}