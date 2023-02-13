using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;

    public static AudioManager Instance { get { return _instance; } }

    public AudioClip[] clips;
    private AudioSource playerSource;
    private AudioSource objectSource;
    private AudioSource musicSource;
    private GameMaster gameMaster;
    private float musicVolume = 0.2f;
    private float effectVolume = 1f;
    private bool onTitle = true;
    private int effectSliderSFXIndex = 0;

    public bool SetOnTitle
    {
        set
        {
            onTitle = value;
        }
    }

    public float MusicVolume
    {
        set 
        {
            gameMaster.SetVolumePref(value, true);
            musicVolume = Mathf.Clamp(0.2f * value, 0, 0.2f); 

            if(musicSource != null) musicSource.volume = musicVolume;
        }
    }

    public float EffectVolume
    {
        set
        {
            float oldEffectVolume = effectVolume;
            gameMaster.SetVolumePref(value, false);

            effectVolume = EffectVolumeNoSFX(value);

            //Play sound for user convience when adjusting audio
            if (!objectSource.isPlaying && !Mathf.Approximately(oldEffectVolume, effectVolume))
            {
                effectSliderSFXIndex += effectVolume > oldEffectVolume ? 1 : -1 + 4;
                effectSliderSFXIndex %= 4;

                PlayObjectAudioClip(effectSliderSFXIndex + 7);
            }
        }
    }

    public float EffectVolumeNoSFX(float value)
    {
        float clampedValue = Mathf.Clamp01(value);
        effectVolume = clampedValue;

        if (playerSource != null && objectSource != null)
        {
            playerSource.volume = clampedValue;
            objectSource.volume = clampedValue;
        }

        return clampedValue;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            gameMaster = GameObject.Find("GameMaster").GetComponent<GameMaster>();
            AudioSource[] sources = GetComponents<AudioSource>();
            playerSource = sources[0];
            objectSource = sources[1];
            musicSource = sources[2];

            StartCoroutine("PlayMusicLoops");
        }
    }

    public void PlayPlayerAudioClip(int index)
    {
        playerSource.clip = clips[Mathf.Clamp(index, 0, clips.Length - 1)];

        if (index == 0) playerSource.pitch = Random.Range(1, 1.05f);
        else playerSource.pitch = 1;

        playerSource.Play();
    }

    public void PlayObjectAudioClip(int index)
    {
        objectSource.clip = clips[index];

        objectSource.Play();
    }

    IEnumerator PlayMusicLoops()
    {
        yield return null;

        for(int i = 11; i< 14; i++){
            musicSource.clip = clips[i];
            musicSource.Play();

            if(i == 11)
            {
                musicSource.loop = true;
                while (onTitle)
                {
                    yield return null;
                }
                musicSource.loop = false;
                musicSource.Stop();
            }

            if(i != 13) {
                while (musicSource.isPlaying)
                {
                    yield return null;
                }
            }
            else
            {
                musicSource.loop = true;
            }
        }
    }

    public void StartGameMusic() { onTitle = false; }

    public void StartTitleMusic() 
    { 
        onTitle = true;
        StopAllCoroutines();
        musicSource.Stop();
        StartCoroutine(PlayMusicLoops());
    }

    public void PlayFinalle() {
        StartCoroutine("PlayFinalleMusic");
    }

    IEnumerator PlayFinalleMusic()
    {
        yield return null;

        musicSource.clip = clips[14];
        musicSource.loop = false;
        float currentVolume = musicSource.volume;
        musicSource.volume = Mathf.Clamp01(currentVolume * 1.5f);
        
        musicSource.Play();

        while (musicSource.isPlaying)
        {
            yield return null;
        }

        musicSource.clip = clips[13];
        musicSource.loop = true;
        musicSource.volume = currentVolume;

        musicSource.Play();
    }
}
