using UnityEngine;
using UnityEngine.UI;

public class PauseBrightnessController : MonoBehaviour
{
    public Slider pauseBrightnessSlider; // Arrástralo desde el inspector

    void OnEnable()
    {
        // Sincronizar valor inicial
        float current = PlayerPrefs.GetFloat("Brightness", 0f);
        pauseBrightnessSlider.SetValueWithoutNotify(current);

        // Añadir listener para guardar y aplicar
        pauseBrightnessSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnDisable()
    {
        // Eliminar listener para evitar duplicados
        pauseBrightnessSlider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    void OnSliderChanged(float value)
    {
        // Aplicar a la postExposure
        var vol = FindFirstObjectByType<UnityEngine.Rendering.Volume>();
        if (vol != null && vol.profile.TryGet<UnityEngine.Rendering.Universal.ColorAdjustments>(out var ca))
            ca.postExposure.value = value;

        // Guardar el valor para todas las escenas y futuras sesiones
        PlayerPrefs.SetFloat("Brightness", value);
        PlayerPrefs.Save();
    }
}
