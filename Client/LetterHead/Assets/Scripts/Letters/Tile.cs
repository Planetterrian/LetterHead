using System;
using PathologicalGames;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using uTools;

public class Tile : MonoBehaviour
{
    public Image image;
    public Image gloss;
    public uTweenAlpha glossTween;
    public uTweenRotation shakeTween;
    public uTweenPosition slotRevealTween;
    public TextMeshProUGUI letterText;

    public LetterDefinition letterDefinition;

    public Color selectedColor;
    public Color originalUsedColor;
    private bool selected;

    public int ID;
    public int referencedTileID;

    public bool moveTospellerAnimationCompleted;

    public int xBoardPos;
    public int yBoardPos;

    public uTweenScale popInTween;

    public AudioSource selectSound;
    public AudioSource deselectSound;

    private static int currentId = 1;

    private Vector2 startPosition;
    private Vector2 destinationPosition;
    private bool isAnimating;
    private float moveAnimationTime;
    private float animateStartTime;
    private CanvasGroup canvasGroup;
    private Color originalLetterColor;
    private Color originalImageColor;
    private Action animationCompleteCallback;

    public GameObject starburst;
    public GameObject glow;


    public enum TileMode
    {
        Grid, SpelledWord, Wild
    }


    private TileMode mode;
    private bool disableInteractionWhileAnimating;

    public TileMode Mode
    {
        get { return mode; }
        set
        {
            mode = value;
        }
    }

    private bool enabledState;
    private bool hasBeenUsed;

    void OnSpawned()
    {
        hasBeenUsed = false;
        letterText.color = originalLetterColor;
        image.color = originalImageColor;
        
        SetEnabled(true);

        gloss.gameObject.SetActive(false);


        moveTospellerAnimationCompleted = false;

        if(starburst)
            starburst.gameObject.SetActive(false);

        if(glow)
            glow.SetActive(false);

        slotRevealTween.ResetToEndState();
    }

    public void HideLetter()
    {
        slotRevealTween.ResetToInitialState();
    }

    public void RevealLetter(float delay)
    {
        TimerManager.AddEvent(delay, () => slotRevealTween.Play());
    }

    public void StarburstFadeout()
    {
        starburst.SetActive(true);
        TimerManager.AddEvent(0.5f, PopOut);
    }

	// Update is called once per frame
	void Awake ()
	{
        canvasGroup = GetComponent<CanvasGroup>();
	    ID = currentId;
        currentId++;

        originalLetterColor = letterText.color;
        originalImageColor = image.color;
	}

    void Update()
    {
        if (isAnimating)
        {
            var progress = (Time.timeSinceLevelLoad - animateStartTime)/(moveAnimationTime);
            var newPos = Vector2.Lerp(startPosition, destinationPosition, progress);
            transform.localPosition = newPos;

            if (progress > 0.999f)
            {
                if (disableInteractionWhileAnimating)
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }

                isAnimating = false;
                transform.localPosition = destinationPosition;
                if (animationCompleteCallback != null)
                    animationCompleteCallback();
            }
        }
    }

    public void SetEnabled(bool state)
    {
        enabledState = state;
        if (state)
        {
            image.color = originalImageColor;
        }
        else
        {
            image.color = new Color(0.7f, 0.7f, 0.7f);
        }
    }

    public void PopIn(Vector3 newScale)
    {
        popInTween.to = newScale;
        popInTween.Rewind();
        popInTween.Play();
    }

    public void PulseGlow()
    {
        glow.SetActive(true);

        var tweenA = glow.GetComponent<uTweenScale>();
        tweenA.Rewind();
        tweenA.Play();
    }

    internal void SetLetter(LetterDefinition _letterDefinition, Sprite tileSprite)
    {
        letterDefinition = _letterDefinition;

        if (tileSprite != null)
            image.sprite = tileSprite;

        letterText.text = letterDefinition.letter.ToUpper();
    }
    
    public void OnClicked()
    {
        if (Mode == Tile.TileMode.Grid)
        {
            if (!GameGui.Instance.CanClickBoardTile())
                return;

            if (!CanBeSelected()) 
                return;

            if (IsSelected())
            {
                BoardManager.Instance.DeselectTile(this);
                deselectSound.Play();
            }
            else
            {
                BoardManager.Instance.SelectTile(this);
                selectSound.Play();
            }
        }
        else if (Mode == Tile.TileMode.SpelledWord)
        {
            if (!GameGui.Instance.CanClickSpellerTile())
                return;

            var originalTile = BoardManager.Instance.GetTileById(referencedTileID);

            if (originalTile)
            {
                BoardManager.Instance.DeselectTile(originalTile);
            }

            deselectSound.Play();
        }
    }

    public bool IsEnabled()
    {
        return enabledState;
    }

    public bool CanBeSelected(bool allowAutomated = false)
    {
        if (!enabledState)
        {
            return false;
        }
        
        return true;
    }

    public bool IsSelected()
    {
        return selected;
    }

    public void Deselect()
    {
        selected = false;
        UpdateLetterColor();
    }

    public void Select()
    {
        selected = true;
        UpdateLetterColor();
    }


    private void UpdateLetterColor()
    {
        if (selected)
        {
            letterText.color = selectedColor;
        }
        else
        {
            if(hasBeenUsed)
                letterText.color = originalUsedColor;
            else
                letterText.color = originalLetterColor;
        }
    }

    public void SetColor(Color color)
    {
        image.color = color;
    }

    public void AnimateToPosition(Vector2 position, float time = 1, Action _animationCompleteCallback = null, bool disableInteraction = true)
    {
        animationCompleteCallback = _animationCompleteCallback;
        disableInteractionWhileAnimating = disableInteraction;

        if (disableInteraction)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        startPosition = transform.localPosition;
        animateStartTime = Time.timeSinceLevelLoad;
        moveAnimationTime = time;
        destinationPosition = position;
        isAnimating = true;
    }

    public void PopOut()
    {
        popInTween.Play(PlayDirection.Reverse);
        Delete(1);
    }

    public void Delete(float delay = 0)
    {
        PoolManager.Pools["UI"].Despawn(transform, delay);
    }

    public void Shake(float duration = 3f)
    {
        if(shakeTween.enabled)
            return;

        shakeTween.Play();

        TimerManager.AddEvent(duration, () =>
        {
            shakeTween.enabled = false;
            transform.rotation = Quaternion.identity;
        });
    }

    public void DoGloss()
    {
        gloss.gameObject.SetActive(true);
        glossTween.Rewind();
        glossTween.Play();
    }

    public void CancelPopIn()
    {
        popInTween.enabled = false;
    }

    public void MarkAsUsed()
    {
        hasBeenUsed = true;
        UpdateLetterColor();
    }
}
