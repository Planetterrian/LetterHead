using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CanvasLetterboxScaler : MonoBehaviour
{
    public bool setCamera;

	// Use this for initialization
	void Awake ()
	{
	    var cam = Camera.main;

	    var scaler = GetComponent<CanvasScaler>();
	    var newY = scaler.referenceResolution.y / cam.rect.height;
	    scaler.referenceResolution = new Vector2(scaler.referenceResolution.x, newY);
	    OnLevelWasLoaded(0);
	}

    void OnLevelWasLoaded(int level)
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
        
    }
}
