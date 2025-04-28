using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private PlayerController player;

    void Start()
    {
        healthSlider.maxValue = player.BaseHealth;
        healthSlider.value = player.CurrentHealth;
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
        healthSlider.value = newHealth;
    }
}