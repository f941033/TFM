using UnityEngine;

public class AudioSettings : MonoBehaviour
{
    public static AudioSettings Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicAudioSource;
    public AudioSource sfxAudioSource;

    [Header("Defaults")]
    [Range(0f, 1f)] public float defaultMusicVolume = 0.7f;
    [Range(0f, 1f)] public float defaultSFXVolume = 0.8f;

    public static float MusicVolume { get; private set; }
    public static float SFXVolume { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        MusicVolume = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
        SFXVolume = PlayerPrefs.GetFloat("SFXVolume", defaultSFXVolume);
        ApplyMusicVolume();
        ApplySFXVolume();
    }

    public void OnMusicVolumeChanged(float value)
    {
        MusicVolume = value;
        ApplyMusicVolume();
        PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
        PlayerPrefs.Save();
    }

    public void OnSFXVolumeChanged(float value)
    {
        SFXVolume = value;
        ApplySFXVolume();
        PlayerPrefs.SetFloat("SFXVolume", SFXVolume);
        PlayerPrefs.Save();
    }

    void ApplyMusicVolume()
    {
        if (musicAudioSource != null)
            musicAudioSource.volume = MusicVolume;
    }

    void ApplySFXVolume()
    {
        if (sfxAudioSource != null)
            sfxAudioSource.volume = SFXVolume;
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicAudioSource == null || clip == null) return;
        musicAudioSource.Stop();
        musicAudioSource.clip = clip;
        musicAudioSource.loop = loop;
        musicAudioSource.volume = MusicVolume;
        musicAudioSource.Play();
    }

    public static void PlaySFX(AudioSource source, AudioClip clip)
    {
        if (source == null || clip == null) return;
        source.volume = SFXVolume;
        source.PlayOneShot(clip);
    }
}
