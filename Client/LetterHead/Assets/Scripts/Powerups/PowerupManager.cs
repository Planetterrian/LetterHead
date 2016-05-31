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
        var doOverActive = !GameManager.Instance.IsMyRound() && GameManager.Instance.CurrentRound().DoOverUsed;
        powerupButtons[(int)Powerup.Type.DoOver].SetState(doOverActive);
    }

    private void RefreshMyButtons()
    {
        var doOverActive = GameScene.Instance.CurrentState == GameScene.State.Active && GameManager.Instance.IsMyRound() && !GameManager.Instance.CurrentRound().DoOverUsed;
        powerupButtons[(int)Powerup.Type.DoOver].SetState(doOverActive);
    }

    public void RequestUsePowerup(int typeId)
    {
        var powerupType = (Powerup.Type) typeId;

        switch (powerupType)
        {
            case Powerup.Type.DoOver:
                DialogWindowTM.Instance.Show("Do-Over", "Activate Do-Over booster?", UseDoOver, () => { });
                break;
            case Powerup.Type.Shield:
                break;
            case Powerup.Type.StealTime:
                break;
            case Powerup.Type.StealLetter:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        OnRoundStateChanged();
    }

    private void UseDoOver()
    {
        GameRealTime.Instance.OnDoOverUsed();
    }
}