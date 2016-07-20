using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashRedirect : MonoBehaviour
{
    public string sceneName;
    public float minDelay;


    private void Awake()
    {
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(minDelay);
        SceneManager.LoadScene(sceneName);
    }
}