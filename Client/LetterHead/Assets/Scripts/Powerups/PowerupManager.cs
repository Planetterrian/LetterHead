using System;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared;
using UnityEngine;

public class PowerupManager : Singleton<PowerupManager>
{
    public PowerupButton[] powerupButtons;
    public PowerupButton[] opponentpowerupButtons;

    private void Start()
    {
    }

    public void OnRoundStateChanged()
    {
        RefreshMyButtons();
        RefreshOpponentButtons();
    }

    private void RefreshOpponentButtons()
    {
        var doOverActive = !GameManager.Instance.IsMyRound() && GameManager.Instance.CurrentRound() != null && GameManager.Instance.CurrentRound().DoOverUsed;
        opponentpowerupButtons[(int)Powerup.Type.DoOver].SetState(doOverActive);

        var shieldActive = !GameManager.Instance.IsMyRound() && GameManager.Instance.CurrentRound() != null && GameManager.Instance.CurrentRound().ShieldUsed;
        opponentpowerupButtons[(int)Powerup.Type.Shield].SetState(shieldActive);
    }

    private void RefreshMyButtons()
    {
        var type = Powerup.Type.DoOver;
        var doOverActive = GameScene.Instance.CurrentState == GameScene.State.Active && GameManager.Instance.IsMyRound() && !GameManager.Instance.CurrentRound().DoOverUsed && ClientManager.Instance.PowerupCount(type) > 0;

        powerupButtons[(int)type].SetState(doOverActive);
        powerupButtons[(int)type].SetQty(ClientManager.Instance.PowerupCount(type));

        type = Powerup.Type.Shield;
        var shieldActive = GameScene.Instance.CurrentState == GameScene.State.Active && GameManager.Instance.IsMyRound() && !GameManager.Instance.CurrentRound().ShieldUsed && ClientManager.Instance.PowerupCount(type) > 0;
        powerupButtons[(int)type].SetState(shieldActive);
        powerupButtons[(int)type].SetQty(ClientManager.Instance.PowerupCount(type));

        type = Powerup.Type.StealTime;
        var stealTimeActive = GameScene.Instance.CurrentState == GameScene.State.Active && GameManager.Instance.IsMyRound() && !GameManager.Instance.CurrentRound().StealTimeUsed && ClientManager.Instance.PowerupCount(type) > 0;
        powerupButtons[(int)type].SetState(stealTimeActive);
        powerupButtons[(int)type].SetQty(ClientManager.Instance.PowerupCount(type));
    }

    public bool CanUsePowerup(Powerup.Type type)
    {
        if (ClientManager.Instance.PowerupCount(type) < 1)
            return false;

        switch (type)
        {
            case Powerup.Type.DoOver:
                if (GameManager.Instance.MatchDetails.HasDoOverBeenUsed(ClientManager.Instance.myUserInfo.Id))
                    return false;
                break;
            case Powerup.Type.Shield:
                if (GameManager.Instance.MatchDetails.HasShieldBeenUsed(ClientManager.Instance.myUserInfo.Id))
                    return false;
                break;
            case Powerup.Type.StealTime:
                if (GameManager.Instance.MatchDetails.HasStealTimeBeenUsed(ClientManager.Instance.myUserInfo.Id))
                    return false;
                break;
            case Powerup.Type.StealLetter:
                if (GameManager.Instance.MatchDetails.HasStealLetterBeenUsed(ClientManager.Instance.myUserInfo.Id))
                    return false;
                break;
            default:
                throw new ArgumentOutOfRangeException("type", type, null);
        }

        return true;
    }

    public void RequestUsePowerup(int typeId)
    {
        var powerupType = (Powerup.Type) typeId;

        if(!CanUsePowerup(powerupType))
            return;

        switch (powerupType)
        {
            case Powerup.Type.DoOver:
                DialogWindowTM.Instance.Show("Do-Over", "Activate Do-Over booster?", GameRealTime.Instance.OnDoOverUsed, () => { });
                break;
            case Powerup.Type.Shield:
                GameRealTime.Instance.OnShieldUsed();
                break;
            case Powerup.Type.StealTime:
                DoStealTime();
                break;
            case Powerup.Type.StealLetter:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void DoStealTime()
    {
        Srv.Instance.POST("Match/UseStealTime", new Dictionary<string, string>() { {"matchId", GameManager.Instance.MatchDetails.Id.ToString()} }, s =>
        {
            ClientManager.Instance.RefreshMyInfo(false, b =>
            {
                if (b)
                    OnRoundStateChanged();
            });
        });
    }
}