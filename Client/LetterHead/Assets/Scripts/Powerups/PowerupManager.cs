using System;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared;
using LetterHeadShared.DTO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class PowerupManager : Singleton<PowerupManager>
{
    public PowerupButton[] powerupButtons;
    public PowerupButton[] opponentpowerupButtons;

    [HideInInspector]
    public bool stealTimeActive;

    [HideInInspector]
    public bool stealLetterActive;

    private Tile tileToSteal;

    private void Start()
    {
        GameManager.Instance.OnMatchDetailsLoadedEvent.AddListener(OnRoundStateChanged);
    }

    public void OnRoundStateChanged()
    {
        RefreshMyButtons();
        RefreshOpponentButtons();
    }

    private void RefreshOpponentButtons()
    {
        
    }

    private void RefreshMyButtons()
    {
        if(GameManager.Instance.MatchDetails == null)
            return;

        var type = Powerup.Type.DoOver;
        powerupButtons[(int)type].SetState(!GameManager.Instance.MatchDetails.HasDoOverBeenUsed(ClientManager.Instance.UserId()));
        powerupButtons[(int)type].SetQty(ClientManager.Instance.PowerupCount(type));

        type = Powerup.Type.Shield;
        powerupButtons[(int)type].SetState(!GameManager.Instance.MatchDetails.HasShieldBeenUsed(ClientManager.Instance.UserId()));
        powerupButtons[(int)type].SetQty(ClientManager.Instance.PowerupCount(type));

        type = Powerup.Type.StealTime;
        powerupButtons[(int)type].SetState(!GameManager.Instance.MatchDetails.HasStealTimeBeenUsed(ClientManager.Instance.UserId()));
        powerupButtons[(int)type].SetQty(ClientManager.Instance.PowerupCount(type));

        type = Powerup.Type.StealLetter;
        powerupButtons[(int)type].SetState(!GameManager.Instance.MatchDetails.HasStealLetterBeenUsed(ClientManager.Instance.UserId()));
        powerupButtons[(int)type].SetQty(ClientManager.Instance.PowerupCount(type));
    }

    private bool StealActive()
    {
        return stealLetterActive || stealTimeActive;
    }

    public bool CanUsePowerup(Powerup.Type type)
    {
        if (ClientManager.Instance.PowerupCount(type) < 1)
            return false;

        if (GameManager.Instance.MatchDetails == null)
            return false;

        switch (type)
        {
            case Powerup.Type.DoOver:
                if (GameManager.Instance.MatchDetails.HasDoOverBeenUsed(ClientManager.Instance.myUserInfo.Id) || GameManager.Instance.CurrentRound().CurrentState != MatchRound.RoundState.Active)
                    return false;
                break;
            case Powerup.Type.Shield:
                if (GameManager.Instance.MatchDetails.HasShieldBeenUsed(ClientManager.Instance.myUserInfo.Id) || !StealActive())
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

        if (!CanUsePowerup(powerupType))
            return;

        switch (powerupType)
        {
            case Powerup.Type.DoOver:
                DialogWindowTM.Instance.Show("Do-Over", "Activate Do-Over booster?", GameRealTime.Instance.OnDoOverUsed, () => { });
                break;
            case Powerup.Type.Shield:
                if(StealActive())
                    GameRealTime.Instance.RequestUseShield();
                break;
            case Powerup.Type.StealTime:
                DoStealTime();
                break;
            case Powerup.Type.StealLetter:
                DoStealLetter();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void DoStealTime(Action<bool> onUsed = null)
    {
        Srv.Instance.POST("Match/UseStealTime", new Dictionary<string, string>() { { "matchId", GameManager.Instance.MatchDetails.Id.ToString() } }, s =>
        {
            ClientManager.Instance.RefreshMyInfo(false, b =>
            {
                if (b)
                    OnRoundStateChanged();
            });

            if (onUsed != null)
            {
                onUsed(true);
            }
        },
            s =>
            {
                DialogWindowTM.Instance.Error(s);
                if (onUsed != null)
                    onUsed(false);
            }
        );
    }

    public void DoStealLetter(Action<bool> onUsed = null)
    {
        Srv.Instance.POST("Match/UseStealLetter", new Dictionary<string, string>() { { "matchId", GameManager.Instance.MatchDetails.Id.ToString() } }, s =>
        {
            ClientManager.Instance.RefreshMyInfo(false, b =>
            {
                if (b)
                    OnRoundStateChanged();
            });

            if (onUsed != null)
            {
                onUsed(true);
            }
        },
            s =>
            {
                DialogWindowTM.Instance.Error(s);
                if (onUsed != null)
                    onUsed(false);
            }
        );
    }

#if UNITY_EDITOR
    [MenuItem("LetterHead/Steal Time")]
    private static void x()
    {
        Instance.OnStealTimeActivated();
    }
#endif


    public void OnStealLetterActivated(string letterStolen)
    {
        var tile = BoardManager.Instance.GetTileWithLetter(letterStolen);
        stealLetterActive = true;
        tileToSteal = tile;
        GameGui.Instance.chomper.Begin(tile.transform);
        RefreshMyButtons();
    }

    public void OnStealTimeActivated()
    {
        GameGui.Instance.chomper.Begin(GameGui.Instance.timer.transform);
        stealTimeActive = true;
        RefreshMyButtons();
    }

    public void OnChompedClicked()
    {
        if(!GameManager.Instance.IsMyRound())
            return;

        RequestUsePowerup((int)Powerup.Type.Shield);
        RefreshMyButtons();
    }

    public void CancelSteal()
    {
        stealTimeActive = false;
        stealLetterActive = false;
        GameGui.Instance.chomper.Hide();
        RefreshMyButtons();
    }

    public void ChomperBite()
    {
        if (stealTimeActive)
        {
            GameManager.Instance.ReduceTime(20);
        }
        if (stealLetterActive)
        {
            Speller.Instance.RemoveTile(tileToSteal);
            BoardManager.Instance.RemoveTile(tileToSteal, BoardManager.RemoveEffect.Keep);
            tileToSteal.transform.SetParent(GameGui.Instance.chomper.transform, true);
            tileToSteal.transform.SetAsFirstSibling();
        }

        stealTimeActive = false;
        stealLetterActive = false;
        RefreshMyButtons();
    }

}