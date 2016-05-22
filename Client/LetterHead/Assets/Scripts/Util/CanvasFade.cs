using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasFade : MonoBehaviour
{
    public float speed = 3;
    public bool disableInteractionWhenHidden = true;
    public float autoFadeOutDelay = 0;
    public bool startsHidden;
    public bool disableWhenHidden = true;

    private CanvasGroup canvasGroup;

    private int fadeDir = -1;
    private float fadeOutTime = -1;

    public FadeEvent OnFadeOutCompleted;
    public FadeEvent OnFadeInCompleted;

    [Serializable]
    public class FadeEvent : UnityEvent
    {
    }

	// Use this for initialization
	void Awake ()
	{
	    canvasGroup = GetComponent<CanvasGroup>();

	    if (!canvasGroup)
	    {
	        Debug.LogError("Unable to locate canvas group");
	        enabled = false;
            return;
	    }

        if (startsHidden)
        {
            FadeOut(true);
        }
    }

    protected virtual void Start()
    {

    }

    public void FadeIn()
    {
        fadeDir = 0;

        if (disableWhenHidden)
            gameObject.SetActive(true);

        if (autoFadeOutDelay != 0)
        {
            fadeOutTime = Time.time + autoFadeOutDelay;
        }
        else
        {
            fadeOutTime = -1;
        }
    }

    public void FadeOut(bool instant = false)
    {
        if (instant)
        {
            fadeDir = -1;
            canvasGroup.alpha = 0;
            
            FadeOutComplete();
        }
        else
        {
            fadeOutTime = -1;
            fadeDir = 1;
        }
    }

    private void FadeOutComplete()
    {
        if (disableInteractionWhenHidden)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if(disableWhenHidden)
            gameObject.SetActive(false);

        OnFadeOutCompleted.Invoke();
    }

    public float Alpha()
    {
        return canvasGroup.alpha;
    }

    public bool IsFading()
    {
        return fadeDir != -1;
    }

    public bool IsShown()
    {
        return fadeDir == 0 || Alpha() == 1;
    }

    public bool IsHidden()
    {
        return fadeDir == 1 || Alpha() == 0;
    }

    // Update is called once per frame
    void Update () {

	    if (fadeDir == 0)
	    {
	        // Fade In
	        canvasGroup.alpha += Time.deltaTime*speed;

	        if (disableInteractionWhenHidden && !canvasGroup.interactable)
	        {
	            canvasGroup.interactable = true;
	            canvasGroup.blocksRaycasts = true;
	        }

	        if (canvasGroup.alpha >= 1)
	        {
	            canvasGroup.alpha = 1;
                OnFadeInCompleted.Invoke();
	            fadeDir = -1;
	        }
	    }
        else if (fadeDir == 1)
        {
            // Fade Out
            canvasGroup.alpha -= Time.deltaTime * speed;

            if (canvasGroup.alpha <= 0)
            {
                canvasGroup.alpha = 0;
                fadeDir = -1;

                FadeOutComplete();
            }
        }
        else
        {
            if (fadeOutTime > 0 && fadeOutTime < Time.time)
            {
                FadeOut();
            }
        }

	}
}
