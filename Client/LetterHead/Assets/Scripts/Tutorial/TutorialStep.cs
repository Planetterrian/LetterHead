using System;
using LetterHeadShared.DTO;

public class TutorialStep
{
    public int internalCounter;
    public bool requiredActiveGame;
    public GameScene.State? requiredGameState;

    public Action<TutorialManager> RequestAdvance = manager =>
    {
        manager.AdvanceStep();
    };

    public Action<TutorialManager> OnActivated = manager => { };
    public Action<TutorialManager> OnMatchRequestedStart = manager => { };
    public Action<TutorialManager> OnGameStarted = manager => { };
}
