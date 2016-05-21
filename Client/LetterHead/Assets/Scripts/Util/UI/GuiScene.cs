using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasFade), typeof(CanvasGroup))]
public class GuiScene : MonoBehaviour
{
    [HideInInspector]
    public CanvasFade canvasFade;
    private CanvasGroup canvasGroup;
    
    // Use this for initialization
    protected virtual void Awake ()
	{
	    canvasFade = GetComponent<CanvasFade>();
        canvasGroup = GetComponent<CanvasGroup>();

        canvasFade.OnFadeInCompleted.AddListener(OnShown);
        canvasFade.OnFadeOutCompleted.AddListener(OnHidden);
    }
    
    public bool IsShown()
    {
        return canvasGroup.alpha > 0;
    }


    protected virtual void OnHidden()
    {

    }

    public virtual void OnBeginHide()
    {

    }

    protected virtual void OnShown()
    {

    }

    public virtual void OnBeginShow()
    {

    }

    // Update is called once per frame
    void Update () {
	
	}

    public void FadeIn()
    {
        canvasFade.FadeIn();
    }

    public void FadeOut(bool instant = false)
    {
        OnBeginHide();
        canvasFade.FadeOut(instant);
    }
}
