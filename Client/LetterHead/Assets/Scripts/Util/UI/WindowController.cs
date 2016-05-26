using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WindowController : MonoBehaviour
{
    private CanvasFade fade;

    public GameObject modal;

    private void Awake()
    {
        fade = GetComponent<CanvasFade>();
    }

    private void Start()
    {
    }

    public void Show()
    {
        modal.SetActive(false);
        fade.FadeIn();
        SendMessage("OnWindowShown", SendMessageOptions.DontRequireReceiver);
    }

    public void ShowModal()
    {
        modal.SetActive(true);
        fade.FadeIn();
        SendMessage("OnWindowShown", SendMessageOptions.DontRequireReceiver);
    }


    public void Hide(bool instant = false)
    {
        fade.FadeOut(instant);
    }

    public bool IsShown()
    {
        return fade.IsShown();
    }
}