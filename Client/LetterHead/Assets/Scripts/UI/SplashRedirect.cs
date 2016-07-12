using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashRedirect : MonoBehaviour
{
    public string sceneName;

    private void Awake()
    {
    }

    private void Start()
    {
        SceneManager.LoadScene(sceneName);
    }
}