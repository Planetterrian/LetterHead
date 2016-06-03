using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LetterHeadShared;
using uTools;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class LotteryGui : WindowController
{
    public Button spinButton;
    public PowerupsPage powerupsPage;

    public LotteryMachine lotteryMachine;

    private void OnWindowShown()
    {
        spinButton.interactable = true;
        lotteryMachine.Initilize();
    }

    // Use this for initialization
    void Start () {
	    lotteryMachine.onWinAction = OnWin;
	}

    private void OnWin(Powerup.Type type)
    {
        Srv.Instance.POST("User/DailyPowerup", new Dictionary<string, string>() {{"type", ((int) type).ToString()}}, s =>
        {
            ClientManager.Instance.RefreshMyInfo(false, b => powerupsPage.Refresh());
        });
    }

    public void DoSpin()
    {
        spinButton.interactable = false;

        PlayerPrefs.SetString("LastDailyPowerup", DateTime.Now.ToString());
        powerupsPage.Refresh();
        lotteryMachine.Spin();
    }
}
