using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TipDisplay : MonoBehaviour
{
    public string[] tips;

    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        text.text = "Tip: " + tips[UnityEngine.Random.Range(0, tips.Length)];
    }
}