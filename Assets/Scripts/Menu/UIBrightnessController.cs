using UnityEngine;
using UnityEngine.UI;

public class OverlayBrightnessController : MonoBehaviour
{
    public Slider mainBrightnessSlider;    // Slider menú principal
    public Slider pauseBrightnessSlider;   // Slider menú pausa
    public Image brightnessOverlay;        // Panel overlay

    const string PREF_BRIGHTNESS = "Brightness";
    const float DEFAULT = 1f;
    const float MAX_ALPHA = 0.025f;       // Máxima opacidad para no ocultar fondo

    void Awake()
    {
        // Configura rangos
        mainBrightnessSlider.minValue = 0f;
        mainBrightnessSlider.maxValue = 3f;
        pauseBrightnessSlider.minValue = 0f;
        pauseBrightnessSlider.maxValue = 3f;

        // Carga y aplica
        float saved = PlayerPrefs.GetFloat(PREF_BRIGHTNESS, DEFAULT);
        ApplyOverlay(saved);
        SyncSliders(saved);

        // Escucha cambios
        mainBrightnessSlider.onValueChanged.AddListener(OnSliderChanged);
        pauseBrightnessSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnDestroy()
    {
        mainBrightnessSlider.onValueChanged.RemoveListener(OnSliderChanged);
        pauseBrightnessSlider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    void OnSliderChanged(float value)
    {
        PlayerPrefs.SetFloat(PREF_BRIGHTNESS, value);
        PlayerPrefs.Save();

        ApplyOverlay(value);
        SyncSliders(value);
    }

    void SyncSliders(float value)
    {
        if (mainBrightnessSlider.value != value)
            mainBrightnessSlider.SetValueWithoutNotify(value);
        if (pauseBrightnessSlider.value != value)
            pauseBrightnessSlider.SetValueWithoutNotify(value);
    }

    void ApplyOverlay(float value)
    {
        // delta: -1…+2 → normalizar a -1…+1
        float normalized = (value - DEFAULT) / (mainBrightnessSlider.maxValue - DEFAULT);
        normalized = Mathf.Clamp(normalized, -1f, 1f);

        Color overlayColor;
        float alpha = Mathf.Abs(normalized) * MAX_ALPHA;

        if (normalized > 0f)
        {
            // Tinte claro (blanco semitransparente)
            overlayColor = new Color(1f, 1f, 1f, alpha);
        }
        else if (normalized < 0f)
        {
            // Tinte oscuro (negro semitransparente)
            overlayColor = new Color(0f, 0f, 0f, alpha);
        }
        else
        {
            // Neutral: transparente
            overlayColor = new Color(0f, 0f, 0f, 0f);
        }

        brightnessOverlay.color = overlayColor;
    }
}
