using UnityEngine;
using System.Collections;

public abstract class GameManager : Singleton<GameManager>
{
    protected GameScene gameScene;

    protected override void Awake()
    {
        base.Awake();

        gameScene = GameScene.Instance;
    }
    
    public virtual void OnGameStateChanged()
    {
        
    }


    protected virtual void OnGameStarted()
    {
        gameScene.CurrentState = GameScene.State.Active;
    }
}
