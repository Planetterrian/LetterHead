using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class MusicManager : Singleton<MusicManager>
{
    private AudioSource _audio0;
    private AudioSource _audio1;
    private AudioSource currentAudio;

    private float fadePosition = 0;
    private float destinationFadePosition = 0;
    private float maxVolume = 1;
    private float fadeStartTime;
    private float fadeDuration;
    private Coroutine delayPlay;

    public float volume = 0.9f;

    protected override void Awake()
    {
        base.Awake();
        _audio0 = gameObject.AddComponent<AudioSource>();
        _audio0.playOnAwake = false;
        _audio0.loop = true;

        _audio1 = gameObject.AddComponent<AudioSource>();
        _audio1.playOnAwake = false;
        _audio1.loop = true;

        SetMusicVolume(volume);
    }

    // Update is called once per frame
    void Update()
    {
        if (fadePosition != destinationFadePosition)
        {
            fadePosition = Mathf.Lerp((destinationFadePosition == 1 ? 0 : 1), destinationFadePosition, Mathf.Clamp01((Time.time - fadeStartTime) / fadeDuration));
            _audio0.volume = (1 - (fadePosition)) * maxVolume;
            _audio1.volume = fadePosition * maxVolume;

            if (_audio0.volume < 0.0001f && destinationFadePosition == 1)
                _audio0.Pause();

            if (_audio1.volume < 0.0001f && destinationFadePosition == 0)
                _audio1.Pause();
        }
    }

    public void StopMusic()
    {
        PlayMusic(null);
    }
    

    public void SetMusicVolume(float volume)
    {
        maxVolume = volume;

        _audio0.volume = (1 - (fadePosition)) * maxVolume;
        _audio1.volume = fadePosition * maxVolume;
    }

    public void PlayMusic(AudioClip musicClip, float fadeDuration = 1, bool loop = true)
    {
        //Debug.Log("Playing " + musicClip);
        AudioSource audioToPlay = null;

        if (currentAudio == null || currentAudio == _audio1)
        {
            audioToPlay = _audio0;
            destinationFadePosition = 0;
        }
        else
        {
            audioToPlay = _audio1;
            destinationFadePosition = 1;
        }

        fadeStartTime = Time.time;
        this.fadeDuration = fadeDuration;
        audioToPlay.clip = musicClip;

        if (audioToPlay.clip)
            audioToPlay.Play();
        else
            audioToPlay.Stop();

        currentAudio = audioToPlay;
        audioToPlay.loop = loop;

        if (delayPlay != null)
        {
            StopCoroutine(delayPlay);
        }
    }

}
