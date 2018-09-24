using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LetterboxEnabler : MonoBehaviour
{
    public AspectRatioFitter fitter;

    void Awake()
    {
        Debug.Log(Camera.main.aspect);

        if (Camera.main.aspect < fitter.aspectRatio)
        {
            fitter.enabled = true;
            Debug.Log("Enabling letterboxing");
        }
    }

    // Use this for initialization
	void Start () {
	
	}
	
}
