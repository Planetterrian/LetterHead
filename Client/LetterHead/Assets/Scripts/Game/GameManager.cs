using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared;
using LetterHeadShared.DTO;
using Newtonsoft.Json;
using UnityEngine.Events;

public abstract class GameManager : Singleton<GameManager>
{
    private int matchId;
    protected GameScene gameScene;

    protected Match matchDetails;
    public Match MatchDetails
    {
        get { return matchDetails; }
        set { matchDetails = value; }
    }

    public int MatchId
    {
        get
        {
            return matchId;
        }

        set
        {
            matchId = value;
        }
    }

    public UnityEvent OnMatchDetailsLoadedEvent;

    public MatchRound MyCurrentRound()
    {
        return MatchDetails.Rounds.FirstOrDefault(m => m.UserId == ClientManager.Instance.myUserInfo.Id && m.Number == MatchDetails.CurrentRoundNumber);
    }

    public MatchRound CurrentRound()
    {
        if (MatchDetails == null)
            return null;

        return MatchDetails.Rounds.FirstOrDefault(matchRound => matchRound.UserId == MatchDetails.CurrentUserId && matchRound.Number == MatchDetails.CurrentRoundNumber);
    }

    public List<MatchRound> MyRounds()
    {
        if(MatchDetails == null)
            return new List<MatchRound>();

        return MatchDetails.Rounds.Where(m => m.UserId == ClientManager.Instance.myUserInfo.Id).ToList();
    }

    public void LoadMatchDetails(Action onLoadOverrideAction = null)
    {
        Srv.Instance.POST("Match/MatchInfo", new Dictionary<string, string>() {{"matchId", MatchId.ToString()}}, (matchDetailsJson) =>
        {
            MatchDetails = JsonConvert.DeserializeObject<Match>(matchDetailsJson);

            if (onLoadOverrideAction == null)
            {
                MatchDetailsDownloaded();
            }
            else
                onLoadOverrideAction();
        });
    }

    protected virtual void MatchDetailsDownloaded()
    {
        if (IsMyRound())
        {
            if (MyCurrentRound().CurrentState == MatchRound.RoundState.NotStarted)
                gameScene.CurrentState = GameScene.State.Pregame;
            else if (MyCurrentRound().CurrentState == MatchRound.RoundState.WaitingForCategory)
                gameScene.CurrentState = GameScene.State.WaitingForCategory;
        }

        OnMatchDetailsLoaded();
        OnMatchDetailsLoadedEvent.Invoke();
    }

    protected virtual void OnMatchDetailsLoaded()
    {
        GameRealTime.Instance.Connect();
    }

    protected override void Awake()
    {
        base.Awake();

        gameScene = GameScene.Instance;
        GameGui.Instance.timer.OnTimeExpired.AddListener(OnTimerExpired);
    }

    public bool IsMyRound()
    {
        if (MatchDetails == null)
            return false;

        return MatchDetails.CurrentUserId == ClientManager.Instance.myUserInfo.Id;
    }

    private void OnTimerExpired()
    {
        if (IsMyRound())
        {
            SoundManager.Instance.PlayClip("Time Up");
        }

        GameScene.Instance.CurrentState = GameScene.State.WaitingForCategory;
        MyCurrentRound().CurrentState = MatchRound.RoundState.WaitingForCategory;

        if (ScoringManager.Instance.SelectedCategory() != null)
        {
            EndRound();
            SubmitCategory();
        }
    }

    private void EndRound()
    {
        MyCurrentRound().CurrentState = MatchRound.RoundState.Ended;
        GameScene.Instance.CurrentState = GameScene.State.End;
        GameGui.Instance.timer.Stop();
    }

    public int PlayerCount()
    {
        return MatchDetails.Users.Count;
    }

    public void SubmitCategory()
    {
        Srv.Instance.POST("Match/SetCategory", new Dictionary<string, string>()
        {
            { "matchId", MatchId.ToString() },
            { "categoryName", ScoringManager.Instance.SelectedCategory().name }
        }, s =>
        {
            if (CurrentRound().CurrentState == MatchRound.RoundState.Ended)
            {
                LoadMatchDetails(() =>
                {
                    if (PlayerCount() == 1)
                    {
                        TimerManager.AddEvent(1, () => GameScene.Instance.RefreshMatch());
                    }
                    TimerManager.AddEvent(1, () => GameGui.Instance.endRoundWindow.ShowModal());
                });
            }
        });
    }

    public virtual void OnGameStateChanged()
    {
    }


    protected virtual void OnGameStarted()
    {
        GameManager.Instance.MyCurrentRound().CurrentState = MatchRound.RoundState.Active;
        gameScene.CurrentState = GameScene.State.Active;

        BoardManager.Instance.RevealLetters();
    }

    public void StartGame(float timeRemaining)
    {
        GameGui.Instance.timer.StartTimer(timeRemaining);
        OnGameStarted();
    }

    public void OnRealTimeConnected()
    {
        if(IsMyRound() && MyCurrentRound().CurrentState == MatchRound.RoundState.Active)
            GameRealTime.Instance.RequestStart();
    }

    public bool HasCategoryBeenUsed(Category category)
    {
        return MyRounds().Any(c => c.CategoryName == category.name);
    }

    public void SelectCategory(Category category, bool prepopulateGategory = false)
    {
        if (!prepopulateGategory)
        {
            ScoringManager.Instance.OnCategorySelected(category);
            MyCurrentRound().CategoryName = category.name;

            EndRound();
            SubmitCategory();
        }

        CategoryBox.Instance.Refresh();
        CategoryBox.Instance.SetCurrentlySelectedCategory(category);
    }

    public bool CanStart()
    {
        return IsMyRound() && GameScene.Instance.CurrentState == GameScene.State.Pregame && GameManager.Instance.MatchDetails.CurrentState != Match.MatchState.Ended && GameRealTime.Instance.IsConnected();
    }

    public void ReduceTime(float seconds)
    {
        GameGui.Instance.timer.AddTime(-seconds);
    }

    public bool IsDailyMatch()
    {
        return MatchDetails.IsDaily;
    }

    public int OpponentUserId()
    {
        if (MatchDetails == null)
            return -1;

        foreach (var userInfo in MatchDetails.Users)
        {
            if (userInfo.Id != ClientManager.Instance.UserId())
                return userInfo.Id;
        }

        return -1;
    }
}
