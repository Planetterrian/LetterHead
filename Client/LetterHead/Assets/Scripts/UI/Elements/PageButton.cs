using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PageButton : MonoBehaviour
{
    private Button button;

    public Sprite selectedButton;
    public Sprite deselectedButton;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnShown()
    {
        button.image.sprite = selectedButton;
    }

    public void OnHidden()
    {
        button.image.sprite = deselectedButton;
    }
}