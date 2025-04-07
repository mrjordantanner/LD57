using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using DG.Tweening;
using Unity.Services.CloudSave.Models.Data.Player;

public class AudioManager : MonoBehaviour, IInitializable
{
    #region Singleton
    public static AudioManager Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        #endregion


    }
    public string Name { get { return "Audio"; } }

    //public SoundEffect[] soundEffects;

    public bool audioMuted;
    public float soundCooldown;
    //public float pitchRandomizationAmount = 0.2f;

    public SoundBank soundBank;

    [Header("Music")]
    public AudioClip titleScreenMusic;
    public AudioClip gameplayMusic;
    public AudioSource musicAudioSource;
    public float masterVolumeCachedValue;

    [Header("Mixer Groups")]
    public AudioMixerGroup masterMixerGroup;
    //public AudioMixerGroup sfxMixerGroup;
    public AudioMixerGroup musicMixerGroup;

    [Header("Audio Sliders")]
    public AudioSettingsSlider masterSlider;
    public AudioSettingsSlider musicSlider;
    public AudioSettingsSlider soundSlider;

    public IEnumerator Init()
    {

       // soundEffects = Resources.LoadAll<SoundEffect>("SoundEffects");

        musicAudioSource.loop = true;
        musicAudioSource.outputAudioMixerGroup = musicMixerGroup;

        yield return new WaitForSecondsRealtime(0);
    }

    public void SetMixerValue(AudioMixer mixer, string mixerParameter, float percentage)
    {
        var newValue = Utils.ConvertPercentageToDecibels(percentage);
        mixer.SetFloat(mixerParameter, newValue);
    }

    public void PlayMenuSound1()
    {
        soundBank.Menu1.Play();
    }

     public void PlayMenuSound2()
    {
        soundBank.Menu2.Play();
    }

    public void ToggleAudio()
    {
        audioMuted = !audioMuted;
        musicAudioSource.mute = audioMuted;
    }

    public void ReduceMusicVolume()
    {
        musicAudioSource.DOFade(0.5f, 0.3f).SetUpdate(UpdateType.Normal, true);
    }

    public void RestoreMusicVolume()
    {
        musicAudioSource.DOFade(1, 0.3f).SetUpdate(UpdateType.Normal, true);
    }

    public IEnumerator InitializeMusic()
    {
        // TODO
        //print("AudioManager: InitializeMusic() not yet implemented");
        //musicAudioSource.mute = true;
        //foreach (var levelTheme in LevelController.Instance.LevelThemeBank)
        //{
        //    musicAudioSource.clip = levelTheme.music;
        //    musicAudioSource.Play();
        //    yield return new WaitForSecondsRealtime(0.02f);
        //    musicAudioSource.Stop();
        //    yield return new WaitForSecondsRealtime(0.02f);
        //}
        //musicAudioSource.mute = false;

        yield return new WaitForSecondsRealtime(0f);
    }

    public IEnumerator SoundCooldown(SoundEffect sound)
    {
        // TODO is this correct?
        soundCooldown = Time.unscaledDeltaTime;

        sound.canPlay = false;
        yield return new WaitForSecondsRealtime(soundCooldown);
        sound.canPlay = true;
    }

    public IEnumerator MusicTransition(AudioClip track, float extraDelayTime = 0)
    {
        yield return FadeMusicOut();
        yield return new WaitForSecondsRealtime(1.5f + extraDelayTime);
        FadeMusicIn(track);
    }

    // Fading in and out modifies the AudioSource's volume directly so as not to interfere with 
    // the user's audio settings/config
    // In contrast, changing the music volume slider in the UI modifies the musicMixerGroup.audioMixer's volume
    public IEnumerator FadeMusicOut(float fadeDuration = 1.5f)
    {
        print($"AudioManager: Fade Music Out: {musicAudioSource.clip.name}");

        musicAudioSource.DOFade(0, fadeDuration).SetUpdate(UpdateType.Normal, true);
        yield return new WaitForSecondsRealtime(fadeDuration);

        musicAudioSource.Stop();
    }

    public void FadeMusicIn(AudioClip track, float fadeDuration = 1f)
    {
        print($"AudioManager: Fade Music In: {track.name}");

        musicAudioSource.volume = 0;
        musicAudioSource.clip = track;
        musicAudioSource.loop = true;

        musicAudioSource.Play();
        musicAudioSource.DOFade(1, fadeDuration).SetUpdate(UpdateType.Normal, true);

    }
}

