using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class CategoryHighlighter : MonoBehaviour
{
    public float speed;

    private Image image;
    private float animatePct;
    private bool animating;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        if (animating)
        {
            animatePct += Time.deltaTime*speed;

            if (animatePct >= 1)
            {
                animatePct = 1;
                animating = false;
            }

            image.fillAmount = animatePct;
        }
    }

    public void StartAnimation()
    {
        animatePct = 0;
        image.fillAmount = animatePct;
        animating = true;
    }
}
