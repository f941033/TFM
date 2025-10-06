using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VideoSettings : MonoBehaviour
{
    public Slider mainMenuSlider;
    public Slider pauseMenuSlider;
    public Volume globalVolume;       // Tu Volume con ColorAdjustments
    public Image brightnessOverlay;   // Panel overlay negro/blanco

    const string PREF_BRIGHTNESS = "Brightness";
    const float DEFAULT = 1f;
    const float MAX_ALPHA = 0.025f;

    private ColorAdjustments colorAdjust;

    void Awake()
    {
        // Singleton simple
        if (FindObjectsByType<VideoSettings>(FindObjectsSortMode.None).Length > 1) { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);

        // Rango 0–3
        mainMenuSlider.minValue = pauseMenuSlider.minValue = 0f;
        mainMenuSlider.maxValue = pauseMenuSlider.maxValue = 3f;

        // Obtener ColorAdjustments
        if (!globalVolume.profile.TryGet<ColorAdjustments>(out colorAdjust))
            Debug.LogError("ColorAdjustments no hallado.");
    }

    void OnEnable()
    {
        float saved = PlayerPrefs.GetFloat(PREF_BRIGHTNESS, DEFAULT);
        // Inicializar sliders y aplicar
        InitSlider(mainMenuSlider, saved);
        InitSlider(pauseMenuSlider, saved);
    }

    void OnDisable()
    {
        mainMenuSlider.onValueChanged.RemoveListener(OnSliderChanged);
        pauseMenuSlider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void InitSlider(Slider slider, float value)
    {
        slider.SetValueWithoutNotify(value);
        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        // Guardar
        PlayerPrefs.SetFloat(PREF_BRIGHTNESS, value);
        PlayerPrefs.Save();

        // Aplicar brillo global
        if (colorAdjust != null)
            colorAdjust.postExposure.value = value;

        // Overlay tint
        ApplyOverlay(value);

        // Sincronizar el otro slider
        if (mainMenuSlider.value != value)
            mainMenuSlider.SetValueWithoutNotify(value);
        if (pauseMenuSlider.value != value)
            pauseMenuSlider.SetValueWithoutNotify(value);
    }

    private void ApplyOverlay(float value)
    {
        // Normalizar –1…+1
        float norm = (value - DEFAULT) / (mainMenuSlider.maxValue - DEFAULT);
        norm = Mathf.Clamp(norm, -1f, 1f);
        float alpha = Mathf.Abs(norm) * MAX_ALPHA;

        Color col = norm > 0
            ? new Color(1f, 1f, 1f, alpha)
            : norm < 0
                ? new Color(0f, 0f, 0f, alpha)
                : Color.clear;

        brightnessOverlay.color = col;
    }
}
