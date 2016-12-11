using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialManager : Singleton<TutorialManager>
{
    public TextMeshProUGUI tutorialText;
    public GameObject tutorialBubble;
    public GameObject touchToContinue;

    private TutorialStep[] tutorialSteps = new TutorialStep[7];

    public int CurrentStepNumber
    {
        get
        {
            return PlayerPrefs.GetInt("TutorialStep", 0);
        }

        set
        {
            PlayerPrefs.SetInt("TutorialStep", value);
        }
    }

    public int editorStep; // Use -2 to ignore
    private Vector2 lastTouchPos;

    private void Start()
    {
        tutorialSteps[0] = new TutorialStep() // 0
        {
            OnActivated = (tutorialManager) =>
            {
                tutorialManager.SetTutorialMessage("Welcome to Letter Head! \nTouch anywhere to continue the tutorial.",
                    new Vector2(0.5f, 0.5f), true);
                DisablePageButtons();
                MenuGui.Instance.dashboardScene.newGamePage.EnableOnlySoloGame();
            }
        };

        tutorialSteps[1] = new TutorialStep() // 1
        {
            OnActivated = (tutorialManager) =>
            {
                tutorialManager.SetTutorialMessage("Start a new solo game by tapping the solo game button now.",
                    new Vector2(0.5f, 0.75f));
                DisablePageButtons();
                MenuGui.Instance.dashboardScene.pagination.SetCurrentPage(PersistManager.NewGamePage);
                MenuGui.Instance.dashboardScene.newGamePage.EnableOnlySoloGame();
            },
            OnMatchRequestedStart = manager =>
            {
                manager.HideTutorial();
                manager.RequestAdvanceTutorial();
            }
        };

        tutorialSteps[2] = new TutorialStep() // 2
        {
            requiredActiveGame = true,
            OnActivated = manager =>
            {
                manager.SetTutorialMessage(
                    "The goal of Letter Head is to score as many points as you can over 9 rounds.",
                    new Vector2(0.5f, 0.5f), true);
            }
        };

        tutorialSteps[3] = new TutorialStep() // 3
        {
            requiredActiveGame = true,
            OnActivated = manager =>
            {
                SetTutorialMessage(
                                "Start the round by tapping on the lever on the left side of the machine.",
                                new Vector2(0.45f, 0.35f));
            },
            OnGameStarted = manager =>
            {
                manager.RequestAdvanceTutorial();
            }
        };

        tutorialSteps[4] = new TutorialStep() // 4
        {
            OnActivated = manager =>
            {
                manager.SetTutorialMessage(
                    "Spell as many words as you can before the time runs out. Good luck!",
                    new Vector2(0.5f, 0.5f), true);
            },
            RequestAdvance = manager =>
            {
                HideTutorial();
                AdvanceStep();

                PointerEventData pointer = new PointerEventData(EventSystem.current);
                pointer.position = lastTouchPos;

                List<RaycastResult> raycastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, raycastResults);

                foreach (var raycastResult in raycastResults)
                {
                    ExecuteEvents.Execute(raycastResult.gameObject, pointer, ExecuteEvents.pointerClickHandler);
                }
            }
        };

        tutorialSteps[5] = new TutorialStep() // 5
        {
            requiredActiveGame = true,
            requiredGameState = GameScene.State.WaitingForCategory,
            OnActivated = manager =>
            {
                SetTutorialMessage(
                                "Great job! Now you need to select a score for this round. You can hold your finger down on a category for more information. \nTouch anywhere to continue ",
                                new Vector2(0.45f, 0.5f), true);
            }
        };
        
        tutorialSteps[6] = new TutorialStep() // 6
        {
            requiredActiveGame = true,
            requiredGameState = GameScene.State.WaitingForCategory,
            OnActivated = manager =>
            {
                SetTutorialMessage(
                                "You can only use a category once per round so be sure to choose wisely! Touch anywhere to close this message and select your category.",
                                new Vector2(0.45f, 0.5f), true);
            }
        };


        if (Application.isEditor && editorStep != -2)
            CurrentStepNumber = editorStep;
    }

    private bool GameIsActive()
    {
        if (!GameGui.Instance)
            return false;

        return GameManager.Instance.realTimeConnected;
    }

    private TutorialStep CurrentStep()
    {
        if (CurrentStepNumber < 0 || CurrentStepNumber >= tutorialSteps.Length)
            return null;

        return tutorialSteps[CurrentStepNumber];
    }

    public void ActivateCurrentTutorial()
    {
        if (CurrentStepNumber < 0 || CurrentStepNumber >= tutorialSteps.Length)
        {
            HideTutorial();
            return;
        }

        if(CurrentStep().requiredActiveGame && !GameIsActive())
            return;

        if (CurrentStep().requiredGameState.HasValue && GameScene.Instance.CurrentState != CurrentStep().requiredGameState.Value)
            return;

        CurrentStep().OnActivated(this);
    }

    public void HideTutorial()
    {
        touchToContinue.SetActive(false);
        tutorialBubble.SetActive(false);
    }


    public void RequestAdvanceTutorial()
    {
        if (Application.isMobilePlatform)
        {
            for (var i = 0; i < Input.touchCount; ++i)
            {
                lastTouchPos = Input.GetTouch(i).position;
            }
        }
        else
        {
            lastTouchPos = Input.mousePosition;
        }

        CurrentStep().RequestAdvance(this);
    }
    
    private static void DisablePageButtons()
    {
        foreach (var pageButton in MenuGui.Instance.pageButtons)
        {
            pageButton.GetComponent<Button>().interactable = false;
        }
    }

    public bool ShouldDisableNewGameButtons()
    {
        return CurrentStepNumber == 1 || CurrentStepNumber == 0;
    }

    public int ForcedDashboardPage()
    {
        if (CurrentStepNumber == 0 || CurrentStepNumber == 1)
            return PersistManager.NewGamePage;

        return -1;
    }

    public void SetTutorialMessage(string message, Vector2 position, bool allowTouchToContinue = false)
    {
        tutorialBubble.GetComponent<RectTransform>().anchorMin = position;
        tutorialBubble.GetComponent<RectTransform>().anchorMax = position;
        tutorialBubble.SetActive(true);
        tutorialText.text = message;
        touchToContinue.SetActive(allowTouchToContinue);
    }

    public void OnSoloGameClicked()
    {
        if (!Active())
            return;

        CurrentStep().OnMatchRequestedStart(this);
    }

    public void OnRealTimeConnected()
    {
        if(!Active())
            return;

        if (CurrentStep().requiredActiveGame)
            ActivateCurrentTutorial();
    }

    public void OnGameStateChanged()
    {
        if (!Active())
            return;

        if (CurrentStep().requiredGameState.HasValue)
            ActivateCurrentTutorial();
    }

    private bool Active()
    {
        return CurrentStepNumber >= 0;
    }

    public void OnGameStarted()
    {
        if (!Active())
            return;

        CurrentStep().OnGameStarted(this);
    }

    public void AdvanceStep()
    {
        CurrentStepNumber++;

        if (CurrentStepNumber >= tutorialSteps.Length)
        {
            CurrentStepNumber = -1;
            HideTutorial();
            return;
        }

        ActivateCurrentTutorial();
    }

    public bool AllowEarlyScoreSelect()
    {
        return !Active();
    }
}