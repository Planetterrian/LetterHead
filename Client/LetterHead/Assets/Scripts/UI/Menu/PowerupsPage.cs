using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LetterHeadShared;
using Newtonsoft.Json;
using UI.Pagination;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PowerupsPage : Page
{
    public LotteryGui lottetyGui;
    public Button dailyPowerupButton;
    public Button rewardedPowerupButton;

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


    void Update()
    {
        rewardedPowerupButton.interactable = CanDoRewardedAd();
    }

    private bool CanDoRewardedAd()
    {
        if (!AdManager.Instance.HasRewardedVideo())
            return false;

        var date = DateTime.Now.Date.ToString();
        var curDone = PlayerPrefs.GetInt("ad_" + date, 0);
        if (curDone > 4)
            return false;

        return true;
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

    private Powerup.Type GetRandomPowerup()
    {
        return (Powerup.Type)Random.Range(0, 4);
    }

    private string PowerupName(Powerup.Type type)
    {
        switch (type)
        {
            case Powerup.Type.DoOver:
                return "Do-Over";
                break;
            case Powerup.Type.Shield:
                return "Shield";
                break;
            case Powerup.Type.StealTime:
                return "Steal Time";
                break;
            case Powerup.Type.StealLetter:
                return "Steal Letter";
                break;
        }

        return "";
    }

    public void WatchAdClicked()
    {
        AdManager.Instance.ShowRewardedVideo(reward =>
        {
            var boosterType = GetRandomPowerup();

            var date = DateTime.Now.Date.ToString();
            var curDone = PlayerPrefs.GetInt("ad_" + date, 0);
            PlayerPrefs.SetInt("ad_" + date, curDone + 1);
            Debug.Log("Cur Done = " + PlayerPrefs.GetInt("ad_" + date, 0));
            Debug.Log("Date = " + date);

            Srv.Instance.POST("User/RewardedAd", new Dictionary<string, string>() { { "type", ((int)boosterType).ToString() } }, s =>
            {
                ClientManager.Instance.RefreshMyInfo(false, b => Refresh());
            }, DialogWindowTM.Instance.Error);

            DialogWindowTM.Instance.Show("Completed!", "You have been awarded a free " + PowerupName(boosterType) + " booster!", () => { });
        });
    }
}
