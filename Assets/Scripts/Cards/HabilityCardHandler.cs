using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class HabilityCardHandler : MonoBehaviour, IPointerClickHandler
{
    public BuffCardData cardData;
    public PlayerController player;
    public float cooldownRemaining;
    [SerializeField] private Image overlayImage;
    [SerializeField] private TextMeshProUGUI warningCooldown;
    private Button button;

    public void Initialize(BuffCardData card, PlayerController player)
    {
        this.cardData = card;
        this.player = player;
        overlayImage = transform.Find("CooldownOverlay").GetComponent<Image>();
        overlayImage.fillAmount = 0;
        button = GetComponent<Button>();
        button.gameObject.SetActive(true);
        button.interactable = true;
        Debug.Log("he inicializado el overlay");
    }
    // Update is called once per frame
    void Update()
    {
        if (cooldownRemaining > 0f)
        {
            cooldownRemaining -= Time.deltaTime;
            overlayImage.fillAmount = cooldownRemaining / cardData.cooldown;
            if (cooldownRemaining <= 0f)
            {
                overlayImage.fillAmount = 0;
                GetComponent<Button>().interactable = true;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (cooldownRemaining > 0f)
        {
            FindFirstObjectByType<GameManager>().ShowMessage("¡Tiene cooldown, no se puede activar!", 2);
            return;
        }
        cardData.Play(player, Vector3.zero);
        cooldownRemaining = cardData.cooldown;
        overlayImage.fillAmount = 1f;
        GetComponent<Button>().interactable = false;
    }
}
