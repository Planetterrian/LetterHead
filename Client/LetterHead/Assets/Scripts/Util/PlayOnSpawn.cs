using UnityEngine;
using System.Collections;

public class PlayOnSpawn : MonoBehaviour
{
    private AudioSource[] audioSource;

	// Use this for initialization
	void Awake ()
	{
	    audioSource = GetComponents<AudioSource>();
	}

    void OnSpawned()
    {
        foreach (var source in audioSource)
        {
            source.Play();            
        }
    }
}
