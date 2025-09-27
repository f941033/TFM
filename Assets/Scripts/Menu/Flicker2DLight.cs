using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Flicker2DLight : MonoBehaviour
{
    public Light2D light2D;
    public float minIntensity = 2f;
    public float maxIntensity = 20f;
    public float flickerSpeed = 0.2f; // Cuanto menor, más rápido cambia

    float targetIntensity;
    float timer;

    void Start()
    {
        if (light2D == null)
            light2D = GetComponent<Light2D>();

        targetIntensity = light2D.intensity;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > flickerSpeed)
        {
            targetIntensity = Random.Range(minIntensity, maxIntensity);
            timer = 0f;
        }
        // Lerp para suavizar el cambio de intensidad
        light2D.intensity = Mathf.Lerp(light2D.intensity, targetIntensity, Time.deltaTime * 8f);
    }
}
