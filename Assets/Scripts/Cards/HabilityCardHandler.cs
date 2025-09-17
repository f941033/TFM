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
            button.gameObject.SetActive(true);
            button.interactable = true;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickBuff);
        }
        else
        {
            button.gameObject.SetActive(true);
        }
    }

    public void OnClickBuff()
    {
        if (cardData is BuffCardData buff)
        {
            if (cooldownRemaining > 0f)
            {
                FindFirstObjectByType<GameManager>().ShowMessage("The power you crave is on cooldown and it denies you its strength.", 4);
                return;
            }
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
        if (cooldownText != null){} cooldownText.text = GetCooldownValue().ToString();

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
