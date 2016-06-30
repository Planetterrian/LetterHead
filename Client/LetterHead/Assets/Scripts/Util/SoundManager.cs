using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;

public class SoundManager : Singleton<SoundManager>
{
    public List<AudioClip> clips;

    private Dictionary<string, AudioClip> clipDictionary = new Dictionary<string, AudioClip>();

    public List<AudioSource> audioSources;

	// Use this for initialization
	void Start () {
	    foreach (var audioClip in clips)
	    {
	        clipDictionary.Add(audioClip.name, audioClip);
	    }

	    if (Muted())
	    {
	        ToggleSound(false);
	    }
	}

    public void ToggleSound(bool state)
    {
        //AudioListener.volume = state ? 1 : 0;
        PlayerPrefs.SetInt("Muted", state ? 0 : 1);
    }


    public bool Muted()
    {
        return !PersistManager.Instance.SoundEnabled;
    }

    public void PlayClip(string name)
    {
        if(Muted())
            return;

        var clip = clipDictionary[name];

        _PlayClip(clip);
    }

    private void _PlayClip(AudioClip clip)
    {
        foreach (var audioSource in audioSources)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = clip;
                audioSource.Play();
                return;
            }
        }
    }
}
