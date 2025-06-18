using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private PlayerController player;
    [SerializeField] private TextMeshProUGUI healthValueText;

    void Start()
    {
        healthSlider.maxValue = player.CurrentHealth;
        healthSlider.value = player.CurrentHealth;
        healthValueText.text = $"{player.CurrentHealth}/{player.BaseHealth}";
    }

    void OnEnable()
    {
        player.OnHealthChanged += UpdateHealthBar;
    }

    void OnDisable()
    {
        player.OnHealthChanged -= UpdateHealthBar;
    }

    private void UpdateHealthBar(float newHealth)
    {
        healthSlider.maxValue = player.BaseHealth;
        healthSlider.value = newHealth;
        healthValueText.text = $"{newHealth}/{player.BaseHealth}";
    }
}