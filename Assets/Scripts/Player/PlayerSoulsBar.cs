using UnityEngine;
using UnityEngine.UI;

public class PlayerSoulsBar : MonoBehaviour
{
    [SerializeField] private Slider soulsSlider;
    [SerializeField] private PlayerController player;
    [SerializeField] private RectTransform ticksContainer;
    [SerializeField] private GameObject tickPrefab;

    private float maxSouls;
    void Awake()
    {
        maxSouls = player.MaxSouls;
        soulsSlider.maxValue = player.MaxSouls;
        soulsSlider.minValue = 0;
        soulsSlider.value = player.CurrentSouls;
    }

    void OnEnable()
    {
        player.OnSoulsChanged += UpdateSoulsBar;
    }

    void OnDisable()
    {
        player.OnSoulsChanged -= UpdateSoulsBar;
    }

    private void UpdateSoulsBar(float newSouls)
    {
        soulsSlider.value = newSouls;
    }
    private void CreateTicks()
    {
        // Limpiamos cualquiera que hubiera
        foreach (Transform child in ticksContainer)
            GameObject.Destroy(child.gameObject);

        // Si tienes N almas, sólo debemos dibujar N-1 líneas
        for (int i = 1; i < maxSouls; i++)
        {
            // Normalizamos la posición: i / maxSouls
            float normalizedPos = (float)i / maxSouls;

            // Creamos una marca
            GameObject tick = Instantiate(tickPrefab, ticksContainer);

            // LocalPosition: convertir de normalized a posición local en ticksContainer
            RectTransform rt = ticksContainer.GetComponent<RectTransform>();

            // Determinamos el ancho efectivo del container:
            float containerWidth = rt.rect.width;

            // Calculamos la coordenada X local (en píxeles)
            float xPos = Mathf.Lerp(
                -containerWidth * 0.5f,   // extremo izquierdo
                 containerWidth * 0.5f,   // extremo derecho
                 normalizedPos
            );

            // Ajustamos el RectTransform de la marca
            RectTransform tickRT = tick.GetComponent<RectTransform>();
            tickRT.anchoredPosition = new Vector2(xPos, 0f);
            // Ya en el prefab definiste un tamaño fijo (p. ej. ancho = 2, alto = 20)
        }
    }

}