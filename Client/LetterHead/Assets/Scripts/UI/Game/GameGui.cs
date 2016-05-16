using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameGui : Singleton<GameGui>
{
    public Button submitWordButton;
    public Button clearWordButtons;

    private void Start()
    {
    }

    public bool CanClickBoardTile()
    {
        return true;
    }

    public bool CanClickSpellerTile()
    {
        return true;
    }
}