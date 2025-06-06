﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WindowController : MonoBehaviour
{
    private CanvasFade fade;

    public GameObject modal;

    protected virtual void Awake()
    {
        fade = GetComponent<CanvasFade>();
    }

    private void Start()
    {
    }

    public void Show()
    {
        gameObject.SetActive(true);
        modal.SetActive(false);
        fade.FadeIn();
        SendMessage("OnWindowShown", SendMessageOptions.DontRequireReceiver);
    }

    public void ShowModal()
    {
        gameObject.SetActive(true);
        modal.SetActive(true);
        fade.FadeIn();
        SendMessage("OnWindowShown", SendMessageOptions.DontRequireReceiver);
    }


    public void Hide(bool instant = false)
    {
        fade.FadeOut(instant);
        SendMessage("OnWindowHidden", SendMessageOptions.DontRequireReceiver);
    }

    public bool IsShown()
    {
        return fade.IsShown();
    }
}