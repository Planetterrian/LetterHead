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
#if UNITY_IOS
        minDelay *= 2;
#endif
        yield return new WaitForSeconds(minDelay);
        SceneManager.LoadScene(sceneName);
    }
}