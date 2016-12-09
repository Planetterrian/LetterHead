using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

public class PullToRefresh : MonoBehaviour
{
    public Image centerImage;
    public float pullStartPosition = 10f;
    public float pullEndPosition = 40f;

    public AnimationCurve outerFade;
    public AnimationCurve innerFade;
    public AnimationCurve innerFill;
    public AnimationCurve innerRotation;

    public UnityEvent OnRefresh;

    private Transform contentTransform;
    private Image outerImage;
    private float pullPct;

    // Use this for initialization
    void Start ()
	{
	    contentTransform = transform.parent;
        outerImage = GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update ()
	{
	    var pullPosition = -contentTransform.localPosition.y;

	    pullPosition -= pullStartPosition;
	    var newPullPct = pullPosition/(pullEndPosition - pullStartPosition);
        newPullPct = Mathf.Clamp01(newPullPct);

	    if (newPullPct != pullPct)
	    {
	        pullPct = newPullPct;

            outerImage.color = new Color(outerImage.color.r, outerImage.color.g, outerImage.color.b,
	            outerFade.Evaluate(pullPct));
	        centerImage.color = new Color(centerImage.color.r, centerImage.color.g, centerImage.color.b,
	            innerFade.Evaluate(pullPct));
	        centerImage.fillAmount = innerFill.Evaluate(pullPct);
            centerImage.transform.localEulerAngles = new Vector3(0, 0, innerRotation.Evaluate(pullPct));

        }
	}

    public void OnScrollRelease()
    {
        if(pullPct >= 1)
            OnRefresh.Invoke();
    }
}
