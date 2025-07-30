using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    public PlayerController player;
    public TextMeshProUGUI contadorAlmas;
    public TextMeshProUGUI monedasText;
    public Button upgradeButton;

    private int[] costos = { 50, 100, 250, 500, 1000 };
    private int nivelActual = 0;

    void Start()
    {
        ActualizarUI();
        upgradeButton.onClick.AddListener(MejorarContador);
        monedasText.text = player.amountGold.ToString();
    }

    void MejorarContador()
    {
        if (nivelActual >= costos.Length)
        {
            Debug.Log("¡Has alcanzado el máximo nivel!");
            return;
        }

        int costoActual = costos[nivelActual];

        if (player.amountGold >= costoActual)
        {
            player.amountGold -= costoActual;
            player.IncCurrentSouls();
            player.IncMaxSouls();
            nivelActual++;
            ActualizarUI();
        }
        else
        {
            Debug.Log("No tienes suficientes monedas.");
        }
    }

    void ActualizarUI()
    {
        contadorAlmas.text = player.CurrentSouls + "/" + player.maxSouls;
        monedasText.text = player.amountGold.ToString();
        upgradeButton.interactable = nivelActual < costos.Length;
    }
}

