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
    [SerializeField] private TextMeshProUGUI cooldownText;
    private Coroutine cooldownRoutine;
    private int lastShownSeconds = -1;

    public void Initialize(CardData card, PlayerController player)
    {
        this.cardData = card;

        this.player = player;

        overlayImage.fillAmount = 1f;
        overlayImage.gameObject.SetActive(true);

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
            button.gameObject.SetActive(true);
        }
        Debug.Log("he inicializado el overlay");
    }
    // Update is called once per frame
    void Update()
    {
        /*
        float cooldown = GetCooldownValue();

        if (cooldownRemaining > 0f)
        {
            cooldownRemaining -= Time.deltaTime;
            overlayImage.fillAmount = 1f - Mathf.Clamp01(cooldownRemaining / cooldown);
            if (cooldownRemaining <= 0f)
            {
                overlayImage.fillAmount = 1f;
                button.interactable = true;
            }
        }
        */
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
        cooldownText.gameObject.SetActive(true);
        float cooldown = GetCooldownValue();
        /*
        overlayImage.fillAmount = 0f;
        Debug.Log("Activo el overlay");
        button.gameObject.SetActive(true);
        overlayImage.gameObject.SetActive(true);
        if (button != null)
            button.interactable = false;
        */
        if (cooldown <= 0) return;
        if (cooldownRoutine != null)
            StopCoroutine(cooldownRoutine);

        cooldownRoutine = StartCoroutine(CooldownRoutine(cooldown));
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

        return 0f;
    }

    private IEnumerator CooldownRoutine(float duration)
{
    cooldownRemaining = duration;
    lastShownSeconds = -1;

    // estado inicial
    if (overlayImage != null)
    {
        overlayImage.gameObject.SetActive(true);
        overlayImage.fillAmount = 0f;
    }
   // if (button != null) button.interactable = false;
    UpdateCooldownText(force:true);

    while (cooldownRemaining > 0f)
    {
        cooldownRemaining -= Time.deltaTime;

        if (overlayImage != null)
            overlayImage.fillAmount = 1f - Mathf.Clamp01(cooldownRemaining / duration);

        UpdateCooldownText(); // solo cambia el texto si el entero de segundos ha cambiado

        yield return null; // cada frame
    }

    // Fin del CD
    cooldownRemaining = 0f;
    if (overlayImage != null) overlayImage.fillAmount = 1f;
    if (button != null) button.interactable = true;

    // Limpia o pon "Ready"
    if (cooldownText != null) cooldownText.text = "";

    cooldownRoutine = null;
}

private void UpdateCooldownText(bool force = false)
{
    if (cooldownText == null) return;
    int s = Mathf.CeilToInt(Mathf.Max(0f, cooldownRemaining));
    if (force || s != lastShownSeconds)
    {
        lastShownSeconds = s;
        cooldownText.SetText(s > 0 ? s.ToString() : "");
    }
}
}
