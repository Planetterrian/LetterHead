using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WindowController : MonoBehaviour
{
    private CanvasFade fade;

    protected virtual void Awake()
    {
        fade = GetComponent<CanvasFade>();
    }

    public virtual void Show()
    {
        fade.FadeIn();
    }

    public virtual void Hide()
    {
        fade.FadeOut();
    }
}