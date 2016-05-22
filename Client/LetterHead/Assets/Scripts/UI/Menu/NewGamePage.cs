using System;
using System.Collections.Generic;
using System.Linq;
using UI.Pagination;
using UnityEngine;

public class NewGamePage : Page
{
    public void OnRandomOpponentClicked()
    {
        Srv.Instance.POST("Match/Random", null, s =>
        {
            DialogWindowTM.Instance.Show("New Game", "We're searching for an opponent. You will receive a notification when one is found.", () => { });
        }, DialogWindowTM.Instance.Error );
    }
    
}