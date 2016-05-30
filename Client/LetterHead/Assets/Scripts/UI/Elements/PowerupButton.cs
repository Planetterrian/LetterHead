using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PowerupButton : MonoBehaviour
{
    public Sprite activeSprite;
    public Sprite deactiveSprite;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        SetState(false);
    }

    public void SetState(bool active)
    {
        image.sprite = active ? activeSprite : deactiveSprite;
    }
}