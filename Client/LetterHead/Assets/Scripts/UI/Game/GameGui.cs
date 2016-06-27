using System;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared;
using LetterHeadShared.DTO;
using Newtonsoft.Json;
using TMPro;
using uTools;
using UnityEngine;
using UnityEngine.UI;

public class GameGui : Singleton<GameGui>
{
    public Button submitWordButton;
    public Button clearWordButton;
    public Button shuffleButton;
    public Button startButton;
    public Button nextMatchButton;
    public TextMeshProUGUI nextMatchText;
    public GameObject selectCategoryHelper;
    public TextMeshProUGUI roundNumberLabel;
    public EndRoundWindow endRoundWindow;
    public CategoryBox categoryBox;
    public CategoryBox opponentCategoryBox;
    public CanvasFade opponentCategoryBoxFade;
    public Chomper chomper;

    public TimerElement timer;

    public Lever lever;

    public AvatarBox leftAvatarBox;
    public AvatarBox rightAvatarBox;

    public GameObject dailyGameBox;

    public GameObject leftPowerupsBox;
    public GameObject rightPowerupsBox;

    public GameObject topLightOn;
    public GameObject topLightOff;

    public AudioClip gameplayMusic;

    public uTweenScale shieldTween;
    private Match nextMatchInfo;
    private float lastNextMatchPoll;

    private void Start()
    {
        nextMatchButton.interactable = false;
        nextMatchText.color = new Color(0.3f, 0.3f, 0.3f);
        HidePowerups();
        GameManager.Instance.OnMatchDetailsLoadedEvent.AddListener(OnMatchDetailsLoaded);
        timer.OnTimeExpired.AddListener(OnTimeExpired);
        NextMatchPolling();
    }

    void Update()
    {
        if(Time.time - lastNextMatchPoll > 20)
            NextMatchPolling();
    }

    public bool CanClickBoardTile()
    {
        return GameScene.Instance.CurrentState == GameScene.State.Active;
    }

    public void OnRealTimeConnected()
    {
        OnGameStateChanged();
    }

    public bool CanClickSpellerTile()
    {
        return true;
    }

    public void OnStartClicked()
    {
        GameRealTime.Instance.RequestStart();
        SoundManager.Instance.PlayClip("Lever Start");
    }

    public void ShuffleClicked()
    {
        if(PowerupManager.Instance.stealLetterActive)
            return;

        BoardManager.Instance.Shuffle();
    }

    public void SubmitClicked()
    {
        ScoringManager.Instance.OnWordSubmit();
    }

    public void ClearClicked()
    {
        BoardManager.Instance.ClearCurrentWord();
    }

    public void OnGameStateChanged()
    {
        if (GameScene.Instance.CurrentState == GameScene.State.Pregame)
        {
            startButton.interactable = false;
            shuffleButton.interactable = false;
            selectCategoryHelper.gameObject.SetActive(false);

            if (GameManager.Instance.CanStart())
            {
                startButton.interactable = true;
                lever.SetState(Lever.State.On);
            }
            else
            {
                lever.SetState(Lever.State.Middle);
            }
        }
        else if (GameScene.Instance.CurrentState == GameScene.State.Active)
        {
            startButton.interactable = false;
            shuffleButton.interactable = true;
            lever.SetState(Lever.State.Off);
        }
        else if (GameScene.Instance.CurrentState == GameScene.State.End)
        {
            startButton.interactable = false;
            shuffleButton.interactable = false;
            selectCategoryHelper.gameObject.SetActive(false);
            lever.SetState(Lever.State.Middle);
        }
        else if (GameScene.Instance.CurrentState == GameScene.State.WaitingForCategory)
        {
            startButton.interactable = false;
            shuffleButton.interactable = false;
            selectCategoryHelper.gameObject.SetActive(true);
            lever.SetState(Lever.State.Middle);
        }

        topLightOn.SetActive(GameScene.Instance.CurrentState == GameScene.State.Active);
        topLightOff.SetActive(GameScene.Instance.CurrentState != GameScene.State.Active);

        MusicManager.Instance.PlayMusic(GameScene.Instance.CurrentState == GameScene.State.Active ? gameplayMusic : null);
    }


    private void OnTimeExpired()
    {
        BoardManager.Instance.ClearCurrentWord();
    }

    public static bool CanSelectCategory()
    {
        if (GameScene.Instance.CurrentState == GameScene.State.Pregame || GameScene.Instance.CurrentState == GameScene.State.End)
            return false;

        if (!GameManager.Instance.IsMyRound())
            return false;

        if (GameManager.Instance.MyCurrentRound().CurrentState == MatchRound.RoundState.Ended || GameManager.Instance.MyCurrentRound().CurrentState == MatchRound.RoundState.NotStarted)
            return false;

        return true;
    }

    public void OnMatchDetailsLoaded()
    {
        OnGameStateChanged();

        SetAvatarBox(leftAvatarBox, GameManager.Instance.MatchDetails.Users[0].Id == ClientManager.Instance.UserId() ? 0 : 1);
        timer.SetTimer(GameManager.Instance.MatchDetails.RoundTimeSeconds);
        roundNumberLabel.text = "Round " + (GameManager.Instance.MatchDetails.CurrentRoundNumber + 1) + "/" + GameManager.Instance.MatchDetails.MaxRounds;

        if (GameManager.Instance.MatchDetails.Users.Count > 1)
        {
            rightAvatarBox.gameObject.SetActive(true);
            SetAvatarBox(rightAvatarBox, GameManager.Instance.MatchDetails.Users[0].Id != ClientManager.Instance.UserId() ? 0 : 1);
            ShowPowerups();
        }
        else
        {
            rightAvatarBox.gameObject.SetActive(false);
            HidePowerups();

            if (GameManager.Instance.MatchDetails.IsDaily)
            {
                dailyGameBox.gameObject.SetActive(true);
                dailyGameBox.GetComponentInChildren<TextMeshProUGUI>().text = GameManager.Instance.MatchDetails.DateString;
            }
        }
    }

    private void SetAvatarBox(AvatarBox box, int userIndex)
    {
        box.score.text = GameManager.Instance.MatchDetails.UserScore(userIndex).ToString("N0");
        box.SetAvatarImage(GameManager.Instance.MatchDetails.Users[userIndex].AvatarUrl);
        box.SetName(GameManager.Instance.MatchDetails.Users[userIndex].Username);
    }

    private void HidePowerups()
    {
        leftPowerupsBox.SetActive(false);
        rightPowerupsBox.SetActive(false);
    }

    private void ShowPowerups()
    {
        leftPowerupsBox.SetActive(true);
        rightPowerupsBox.SetActive(true);
    }

    public void OnBackClicked()
    {
        MusicManager.Instance.StopMusic();
        PersistManager.Instance.LoadMenu();
    }

    public void OnNextClicked()
    {
        PersistManager.Instance.LoadMatch(nextMatchInfo.Id, false);
    }

    private void NextMatchPolling()
    {
        Srv.Instance.POST("User/NextAvailableMatch", new Dictionary<string, string>() {{"currentMatchId", GameManager.Instance.MatchId.ToString()}}, s =>
        {
            nextMatchInfo = JsonConvert.DeserializeObject<Match>(s);
            nextMatchButton.interactable = nextMatchButton != null;
            nextMatchText.color = nextMatchButton != null ? Color.black : new Color(0.75f, 0.75f, 0.75f);
        }, null);

        lastNextMatchPoll = Time.time;
    }

    public void OnShieldUsed()
    {
        //shieldTween.gameObject.SetActive(true);
        //shieldTween.ResetToInitialState();
        //shieldTween.Play();

        //TimerManager.AddEvent(1.2f, () => shieldTween.Play(PlayDirection.Reverse));
        //TimerManager.AddEvent(3f, () => shieldTween.gameObject.SetActive(false));
    }

    public void OpponentAvatarDown()
    {
        var round = GameManager.Instance.GetLastOpponentRound();

        if (round != null && !string.IsNullOrEmpty(round.CategoryName))
        {
            //var man = new CategoryManager();
            //categoryBox.SetCurrentlySelectedCategory(man.GetCategory(round.CategoryName));
        }

        opponentCategoryBoxFade.FadeIn();
        opponentCategoryBox.Refresh(GameManager.Instance.OpponentRounds());

    }

    public void OpponentAvatarUp()
    {
        opponentCategoryBoxFade.FadeOut();
    }
}