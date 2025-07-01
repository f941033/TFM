using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class HabilityCardHandler : MonoBehaviour
{
    public CardData cardData;
    public PlayerController player;
    public float cooldownRemaining;
    [SerializeField] private Image overlayImage;
    [SerializeField] private TextMeshProUGUI warningCooldown;
    [SerializeField] private Button button;

    public void Initialize(CardData card, PlayerController player)
    {
        this.cardData = card;

        this.player = player;

        overlayImage.fillAmount = 0;
        overlayImage.gameObject.SetActive(false);

        if (card is BuffCardData)
        {
            Debug.Log("Se que es una carta bufo");
            button.gameObject.SetActive(true);
            button.interactable = true;
            
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickBuff);
        }
        else
        {
            button.gameObject.SetActive(false);
        }
        Debug.Log("he inicializado el overlay");
    }
    // Update is called once per frame
    void Update()
    {
        float cooldown = GetCooldownValue();

        if (cooldownRemaining > 0f)
        {
            cooldownRemaining -= Time.deltaTime;
            overlayImage.fillAmount = cooldownRemaining / cooldown;
            if (cooldownRemaining <= 0f)
            {
                overlayImage.gameObject.SetActive(false);
                overlayImage.fillAmount = 0;
                if (cardData is BuffCardData)
                {
                    button.gameObject.SetActive(true);
                    button.interactable = true;
                }
            }
        }
    }

    public void OnClickBuff()
    {
        Debug.Log("Clico la carta");
        if (cardData is BuffCardData buff)
        {
            if (cooldownRemaining > 0f)
            {
                FindFirstObjectByType<GameManager>().ShowMessage("¡Tiene cooldown, no se puede activar!", 2);
                return;
            }
            Debug.Log("Aplico el bufo");
            buff.Play(player, Vector3.zero);
            cooldownRemaining = buff.cooldown;
            overlayImage.gameObject.SetActive(true);
            StartCooldown();
        }
    }

    public void StartCooldown()
    {
        cooldownRemaining = GetCooldownValue();
        overlayImage.fillAmount = 1f;
        Debug.Log("Activo el overlay");
        button.gameObject.SetActive(true);
        overlayImage.gameObject.SetActive(true);
        if (button != null)
            button.interactable = false;
    }

    float GetCooldownValue()
    {
        if (cardData is BuffCardData buff)
        {
            Debug.Log("la carta es bufo");
            return buff.cooldown;
        }

        if (cardData is HabilityCardData hability)
            return hability.cooldown;

        return 1f;
    }
}
