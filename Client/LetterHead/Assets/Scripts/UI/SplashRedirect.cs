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

    private bool redirecting;

    private void Awake()
    {
    }

    void Update()
    {
        if(Application.isShowingSplashScreen)
            return;

        if(redirecting)
            return;

        redirecting = true;
        StartCoroutine(Redirect());
    }

    private IEnumerator Redirect()
    {
        yield return new WaitForSeconds(minDelay);
        SceneManager.LoadScene(sceneName);
    }
}