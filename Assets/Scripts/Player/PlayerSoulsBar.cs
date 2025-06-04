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

        CreateTicks();
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
        foreach (Transform child in ticksContainer)
            GameObject.Destroy(child.gameObject);

        for (int i = 1; i < maxSouls; i++)
        {
            float normalizedPos = (float)i / maxSouls;

            GameObject tick = Instantiate(tickPrefab, ticksContainer);

            RectTransform rt = ticksContainer.GetComponent<RectTransform>();

            float containerHeight = rt.rect.height;

            float yPos = Mathf.Lerp(
                -containerHeight * 0.5f,
                 containerHeight * 0.5f,
                 normalizedPos
            );

            // Ajustamos el RectTransform de la marca
            RectTransform tickRT = tick.GetComponent<RectTransform>();
            tickRT.anchoredPosition = new Vector2(0f, yPos);
        }
    }

}