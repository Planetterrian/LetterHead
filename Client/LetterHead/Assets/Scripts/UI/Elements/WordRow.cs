using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordRow : MonoBehaviour
{
    public TextMeshProUGUI label;
    public Image background;

    public string word;

    public void SetWord(string word)
    {
        this.word = word;
        label.text = word.ToUpper();
    }
}