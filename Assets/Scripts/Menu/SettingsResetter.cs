using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SettingsResetter : MonoBehaviour
{
    [Header("Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider brightnessSlider;

    [Header("Audio")]
    public UnityEngine.Audio.AudioMixer audioMixer; // Asigna tu mixer (GameAudioMixer)
    public string musicVolumeParam = "MusicVolume"; // Nombre expuesto en mixer
    public string sfxVolumeParam = "SFXVolume";     // Nombre expuesto en mixer

    [Header("Video")]
    public Volume globalVolume;
    public Image brightnessOverlay;

    // Valores por defecto
    public float defaultMusic = 0.7f;
    public float defaultSFX = 0.8f;
    public float defaultBrightness = 1.0f;

    // Nombres clave de PlayerPrefs
    const string MUSIC_KEY = "MusicVolume";
    const string SFX_KEY = "SFXVolume";
    const string BRIGHTNESS_KEY = "Brightness";

    public void ResetAll()
    {
        // Volúmenes audio
        musicSlider.SetValueWithoutNotify(defaultMusic);
        sfxSlider.SetValueWithoutNotify(defaultSFX);
        // Llama OnValueChanged manualmente para forzar update en audioMixer y PlayerPrefs
        musicSlider.onValueChanged?.Invoke(defaultMusic);
        sfxSlider.onValueChanged?.Invoke(defaultSFX);

        // Brillo
        brightnessSlider.SetValueWithoutNotify(defaultBrightness);
        brightnessSlider.onValueChanged?.Invoke(defaultBrightness);

        // PlayerPrefs directos por si acaso
        PlayerPrefs.SetFloat(MUSIC_KEY, defaultMusic);
        PlayerPrefs.SetFloat(SFX_KEY, defaultSFX);
        PlayerPrefs.SetFloat(BRIGHTNESS_KEY, defaultBrightness);
        PlayerPrefs.Save();

        // Extra: forzar valor en audioMixer directamente (opcional si ya lo manejas en OnValueChanged)
        float musicDb = Mathf.Lerp(-40f, 0f, defaultMusic);
        float sfxDb = Mathf.Lerp(-40f, 0f, defaultSFX);
        audioMixer.SetFloat(musicVolumeParam, musicDb);
        audioMixer.SetFloat(sfxVolumeParam, sfxDb);

        // Extra: forzar brillo al post-process (opcional)
        if (globalVolume.profile.TryGet<ColorAdjustments>(out var colorAdjust))
            colorAdjust.postExposure.value = defaultBrightness;

        // Extra: forzar overlay (opcional)
        if (brightnessOverlay != null)
        {
            // Simula el cambio exactamente como en tu controlador de brillo
            float norm = (defaultBrightness - 1f) / (brightnessSlider.maxValue - 1f);
            float alpha = Mathf.Abs(norm) * 0.5f;
            Color col = norm > 0f ? new Color(1, 1, 1, alpha) : norm < 0f ? new Color(0, 0, 0, alpha) : Color.clear;
            brightnessOverlay.color = col;
        }
    }
}
