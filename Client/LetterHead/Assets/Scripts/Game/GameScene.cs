using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class GameScene : Singleton<GameScene>
{
    public SingleplayerGameManager singlePlayer;
    public MultiplayerGameManager multiPlayer;

    [HideInInspector]
    public GameManager gameManager;

    public List<IGameHandler> gameHandlers = new List<IGameHandler>();

    public enum State
    {
        Pregame, Active, WaitingForCategory, End
    }

    private State currentState;
    public State CurrentState
    {
        get { return currentState; }
        set
        {
            currentState = value;
            OnStateChanged.Invoke();
        }
    }

    public UnityEvent OnStateChanged;

    protected override void Awake()
    {
        base.Awake();
        SetForSingleplayer();
    }

    public void AddGameManger(IGameHandler manager)
    {
        gameHandlers.Add(manager);
    }

    private void SetForSingleplayer()
    {
        gameManager = singlePlayer;
        OnGameManagerSet();
    }

    private void SetForMultiplayer()
    {
        gameManager = multiPlayer;
        OnGameManagerSet();
    }

    private void OnGameManagerSet()
    {
        gameManager.gameObject.SetActive(true);
        OnStateChanged.AddListener(gameManager.OnGameStateChanged);
        gameManager.OnMatchDetailsLoadedEvent.AddListener(OnMatchDetailsLoaded);
    }

    void OnMatchDetailsLoaded()
    {
        ResetGame();
    }

    public bool IsGameActive()
    {
        return CurrentState == State.Active;
    }

    public void RefreshMatch()
    {
        gameManager.LoadMatchDetails();
    }

    public void ResetGame()
    {
        foreach (var manager in gameHandlers)
        {
            manager.OnReset();
        }
    }
}
