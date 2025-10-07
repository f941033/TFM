using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;
using DeckboundDungeon.GamePhase;

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
    [SerializeField] private Image DarkReapImage;
    [SerializeField] private Image DarkSpeedImage;
    [SerializeField] private Image ZeroZoneImage;
    [SerializeField] private float blinkThreshold = 5f;
    [SerializeField] private float blinkSpeed = 6f;
    [SerializeField] private float minAlpha = 0.35f;
    private Coroutine blinkRoutine;

    public void Initialize(CardData card, PlayerController player)
    {
        this.cardData = card;

        this.player = player;

        DarkReapImage = GameObject.Find("PlayerStats/StatsPlayer/PowerUp")?.transform.Find("AttkIncr")?.GetComponent<Image>();
        DarkSpeedImage = GameObject.Find("PlayerStats/StatsPlayer/PowerUp")?.transform.Find("SpeedIncr")?.GetComponent<Image>();
        ZeroZoneImage = GameObject.Find("PlayerStats/StatsPlayer/PowerUp")?.transform.Find("ZeroZone")?.GetComponent<Image>();

        overlayImage.fillAmount = 1f;
        overlayImage.gameObject.SetActive(true);

        /*if (card is BuffCardData)
        {
            button.gameObject.SetActive(true);
            button.interactable = true;
        }
        else
        {
            button.gameObject.SetActive(true);
        }*/
    }

    public void StartCooldown()
    {
        var target = GetBlinkTarget();
        if (cardData.cardName == "DARK REAPER")
        {
            if (DarkReapImage)
            {
                DarkReapImage.gameObject.SetActive(true);
            }
        }

        else if (cardData.cardName == "DARK SPEED") { if (DarkSpeedImage) DarkSpeedImage.gameObject.SetActive(true); }

        else if (cardData.cardName == "ZERO ZONE") { if (ZeroZoneImage) ZeroZoneImage.gameObject.SetActive(true); }

        cooldownText.gameObject.SetActive(true);
        float cooldown = GetCooldownValue();
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
        UpdateCooldownText(force: true);

        Image target = GetBlinkTarget();
        if (cooldownRemaining <= blinkThreshold && blinkRoutine == null)
            blinkRoutine = StartCoroutine(BlinkWhileLowTime(target));

        while (cooldownRemaining > 0f)
        {
            cooldownRemaining -= Time.deltaTime;

            if (overlayImage != null)
                overlayImage.fillAmount = 1f - Mathf.Clamp01(cooldownRemaining / duration);

            if (cooldownRemaining <= blinkThreshold && blinkRoutine == null)
                blinkRoutine = StartCoroutine(BlinkWhileLowTime(target));

            UpdateCooldownText();

            yield return null;
        }

        // Fin del CD
        cooldownRemaining = 0f;
        if (cardData.cardName == "DARK REAPER") { if (DarkReapImage) DarkReapImage.gameObject.SetActive(false); }

        else if (cardData.cardName == "DARK SPEED") { if (DarkSpeedImage) DarkSpeedImage.gameObject.SetActive(false); }

        else if (cardData.cardName == "ZERO ZONE") { if (ZeroZoneImage) ZeroZoneImage.gameObject.SetActive(false); }

        if (overlayImage) overlayImage.fillAmount = 1f;

        if (cooldownText != null) cooldownText.text = GetCooldownValue().ToString();

        // parar blink y restaurar alpha por si sigue activo
        if (blinkRoutine != null) { StopCoroutine(blinkRoutine); blinkRoutine = null; }
        if (target) SetAlpha(target, 1f);

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

    private void SetAlpha(Image img, float a)
    {
        if (!img) return;
        var c = img.color; c.a = a; img.color = c;
    }

    private IEnumerator BlinkWhileLowTime(Image target)
    {
        if (!target) yield break;

        while (cooldownRemaining > 0f && cooldownRemaining <= blinkThreshold)
        {
            float t = Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1f);
            float a = Mathf.Lerp(minAlpha, 1f, t);
            SetAlpha(target, a);
            yield return null;
        }
        // restaurar
        SetAlpha(target, 1f);
        blinkRoutine = null;
    }
    private Image GetBlinkTarget()
    {
        if (cardData == null) return null;

        if (cardData.cardName == "DARK REAPER") return DarkReapImage;
        else if (cardData.cardName == "DARK SPEED") return DarkSpeedImage;
        else if (cardData.cardName == "ZERO ZONE") return ZeroZoneImage;

        return overlayImage;
    }
    
    void OnEnable()
    {
        GameManager.OnPhaseChanged += HandlePhaseChanged;
    }

    void OnDisable()
    {
        GameManager.OnPhaseChanged -= HandlePhaseChanged;
        ForceStopCooldown(); // limpia si desactivan el GO a mitad de CD
    }

    private void HandlePhaseChanged(GamePhase newPhase)
    {
        if (newPhase != GamePhase.Action)
            ForceStopCooldown();
    }

    private void ForceStopCooldown()
    {
        if (cooldownRoutine != null) { StopCoroutine(cooldownRoutine); cooldownRoutine = null; }
        if (blinkRoutine    != null) { StopCoroutine(blinkRoutine);    blinkRoutine    = null; }
        cooldownRemaining = 0f;

        HideAllBuffIcons();
    }

    private void HideAllBuffIcons()
    {
        if (DarkReapImage)  DarkReapImage.gameObject.SetActive(false);
        if (DarkSpeedImage) DarkSpeedImage.gameObject.SetActive(false);
        if (ZeroZoneImage)  ZeroZoneImage.gameObject.SetActive(false);
    }
}
