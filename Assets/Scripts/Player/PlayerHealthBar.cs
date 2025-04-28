using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Slider healthSlider;

    [Header("Referencias Player")]
    [SerializeField] private PlayerController player; // arrastra tu PlayerController aquí

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