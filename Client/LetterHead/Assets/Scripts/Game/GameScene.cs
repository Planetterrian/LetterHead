using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class GameScene : Singleton<GameScene>
{
    public SingleplayerGameManager singlePlayer;
    public MultiplayerGameManager multiPlayer;

    [HideInInspector]
    public GameManager gameManager;

    public enum State
    {
        Pregame, Active, End
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
    }

    public bool IsGameActive()
    {
        return true;
    }
}
