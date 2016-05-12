using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class DontDestoryMe : MonoBehaviour 
{
    private static Dictionary<string, GameObject> instances = new Dictionary<string, GameObject>();

	// Use this for initialization
	void Awake () {
	    if (instances.ContainsKey(gameObject.name))
	    {
	        GameObject.DestroyImmediate(gameObject);
            return;
	    }

	    instances[gameObject.name] = gameObject;

        DontDestroyOnLoad(gameObject);
	}
	
}
