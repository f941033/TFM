using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class BrightnessController : MonoBehaviour
{
    [Header("UI Slider")]
    public Slider brightnessSlider;

    [Header("Post-Processing Volume")]
    public Volume postProcessingVolume;

    private ColorAdjustments colorAdjustments;

    void Awake()
    {
        // Obtener ColorAdjustments del Volume Profile
        if (!postProcessingVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            Debug.LogError("No se encontró ColorAdjustments en el Volume Profile.");
        }
    }

    void Start()
    {
        if (colorAdjustments == null) return;

        // Inicializar slider con el valor actual de postExposure
        float currentExposure = colorAdjustments.postExposure.value;
        brightnessSlider.SetValueWithoutNotify(currentExposure);

        // Añadir listener
        brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
    }

    public void OnBrightnessChanged(float value)
    {
        if (colorAdjustments == null) return;

        // Ajustar el post-exposure al valor del slider
        colorAdjustments.postExposure.value = value;
    }

    void OnDestroy()
    {
        if (brightnessSlider != null)
            brightnessSlider.onValueChanged.RemoveListener(OnBrightnessChanged);
    }
}
