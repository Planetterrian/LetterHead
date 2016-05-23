using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;

public class Tooltip : Singleton<Tooltip>
{
    public TextMeshProUGUI mainText;

    private CanvasFade fade;

    protected override void Awake()
    {
        base.Awake();
        fade = GetComponent<CanvasFade>();
    }


    public void Show(string text)
    {
        mainText.text = text;
        fade.FadeIn();
    }
}
