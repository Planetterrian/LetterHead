using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonColorSync : MonoBehaviour
{
    private Button button;
    private Image image;
    private Text text;
    private TextMeshProUGUI textMesh;

    public bool alphaOnly = true;

    private void Awake()
    {
        button = GetComponentInParent<Button>();
        image = GetComponent<Image>();
        text = GetComponent<Text>();
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        var color = button.interactable ? button.colors.normalColor : button.colors.disabledColor;

        if (!alphaOnly)
        {
            if (image)
            {
                
                image.color = color;
            }
            if (text)
            {
                text.color = color;
            }
            if (textMesh)
            {
                textMesh.color = color;
            }
        }
        else
        {
            if (image && image.color.a != color.a)
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, color.a);
            }
            if (text && text.color.a != color.a)
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, color.a);
            }
            if (textMesh && textMesh.color.a != color.a)
            {
                textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, color.a);
            }
        }
    }
}