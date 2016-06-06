using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class Tooltip : Singleton<Tooltip>
{
    public TextMeshProUGUI mainText;

    private CanvasFade fade;

    protected override void Awake()
    {
        base.Awake();
        fade = GetComponent<CanvasFade>();
    }


    public void Show(string text, float yPos)
    {
        transform.position = new Vector3(transform.position.x, yPos, transform.position.z);
        mainText.text = text;
        fade.FadeIn();
    }

    public void Hide()
    {
        fade.FadeOut();
    }
}
