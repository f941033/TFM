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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (cooldownRemaining > 0f)
        {
            //StartCoroutine(ShowMessage("¡Carta en cooldown!", 1.5f));
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
            Debug.Log("El remaining está distinto de 0");
            return;
        }
        Debug.Log("El remaining es 0");
        cardData.Play(player, Vector3.zero);
        cooldownRemaining = cardData.cooldown;
        overlayImage.fillAmount = 1f;
        GetComponent<Button>().interactable = false;
    }

    private IEnumerator ShowMessage(string message, float duration)
    {
        warningCooldown.text = message;
        warningCooldown.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        warningCooldown.gameObject.SetActive(false);
    }
}
