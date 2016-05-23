using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistManager : Singleton<PersistManager>
{
    public Canvas persistCanvas;

    public int matchToLoadId;
    public bool matchToLoadIsDaily;
    

    private void Start()
    {
        persistCanvas.gameObject.SetActive(true);
    }

    public void LoadMatch(int matchId, bool isDaily)
    {
        matchToLoadId = matchId;
        matchToLoadIsDaily = isDaily;

        SceneManager.LoadScene("game");
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("menu");
    }
}