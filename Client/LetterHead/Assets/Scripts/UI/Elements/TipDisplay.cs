using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

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
        StartCoroutine(GetTip());
    }

    private IEnumerator GetTip()
    {
        var www = UnityWebRequest.Get("http://letterhead.azurewebsites.net/Home/Tip");
        yield return www.Send();

        if(!www.isError && text)
            text.text = www.downloadHandler.text;
    }
}