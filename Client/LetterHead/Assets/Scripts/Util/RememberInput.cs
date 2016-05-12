using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class RememberInput : MonoBehaviour
{
    private InputField input;

    private void Awake()
    {
        input = GetComponent<InputField>();
    }

    private void Start()
    {
        input.text = PlayerPrefs.GetString(gameObject.name, input.text);
    }

    public void OnChanged()
    {
        PlayerPrefs.SetString(gameObject.name, input.text);
    }
}