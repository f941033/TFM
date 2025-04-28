using UnityEngine;
using UnityEngine.UI;

public class PlayerSoulsBar : MonoBehaviour
{
    [SerializeField] private Slider soulsSlider;
    [SerializeField] private PlayerController player;

    void Awake()
    {
        soulsSlider.maxValue = player.MaxSouls;
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
}