using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LetterHeadShared;
using Newtonsoft.Json;
using UI.Pagination;
using UnityEngine;
using UnityEngine.UI;

public class PowerupsPage : Page
{
    public LotteryGui lottetyGui;
    public Button dailyPowerupButton;

    public PowerupRow[] powerupRows;

    public void Refresh()
    {
        for (int index = 0; index < powerupRows.Length; index++)
        {
            var powerupRow = powerupRows[index];
            powerupRow.SetCount(ClientManager.Instance.PowerupCount((Powerup.Type)index));
        }

        dailyPowerupButton.interactable = false;

        Srv.Instance.POST("User/CanDoDailyPowerup", null, s =>
        {
            var can = JsonConvert.DeserializeObject<bool>(s);
            dailyPowerupButton.interactable = can;
        });
    }


    void OnApplicationFocus(bool state)
    {
        if (state)
        {
            Refresh();
        }
    }

    void Start()
    {
        IapManager.Instance.OnItemsUpdated.AddListener(OnIapsUpdated);
    }

    private void OnIapsUpdated()
    {
        ClientManager.Instance.RefreshMyInfo(false, b => Refresh());
    }


    public void DailyBoosterClicked()
    {
        lottetyGui.ShowModal();
    }

    public void WatchAdClicked()
    {
        
    }
}
