using UnityEngine;
using UnityEngine.UI;

public class OptionsPanelController : MonoBehaviour
{
    [Header("Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    private AudioSettings audioSettings;

    void Awake()
    {
        audioSettings = AudioSettings.Instance;
        if (audioSettings == null)
            Debug.LogError("AudioSettings no encontrado en la escena");
    }

    void Start()
    {
        SetupSliders();
    }

    void OnEnable()
    {
        SetupSliders();
    }

    void OnDisable()
    {
        RemoveListeners();
    }

    void OnDestroy()
    {
        RemoveListeners();
    }

    private void SetupSliders()
    {
        if (audioSettings == null || musicSlider == null || sfxSlider == null)
            return;

        RemoveListeners();

        // Sincroniza valores sin disparar eventos
        musicSlider.SetValueWithoutNotify(AudioSettings.MusicVolume);
        sfxSlider.SetValueWithoutNotify(AudioSettings.SFXVolume);

        // Añade listeners a la instancia singleton
        musicSlider.onValueChanged.AddListener(audioSettings.OnMusicVolumeChanged);
        sfxSlider.onValueChanged.AddListener(audioSettings.OnSFXVolumeChanged);
    }

    private void RemoveListeners()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveAllListeners();
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveAllListeners();
    }
}
