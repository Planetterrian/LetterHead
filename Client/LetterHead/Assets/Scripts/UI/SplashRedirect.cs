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
    private float startTime;

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
        startTime = Time.time;
        var async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        async.allowSceneActivation = false;
        while (async.progress < 0.9f)
        {
            yield return null;
        }

        var duration = Time.time - startTime;
        Debug.Log("Load time: " + duration);
        if(duration < minDelay)
            yield return new WaitForSeconds(minDelay - duration);

        async.allowSceneActivation = true;
    }
}