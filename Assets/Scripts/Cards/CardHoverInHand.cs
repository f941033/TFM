using DeckboundDungeon.GamePhase;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class CardHoverInHand : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuración")]
    public Color hoverColor = new Color(1f, 0.9f, 0.6f, 1f);
    private Color originalColor;

    [Header("Referencias")]
    public Image cardImage;
    private CardSelector cardSelector;

    // Hover en escala y animación
    private RectTransform rectTransform;
    private Vector3 originalScale;
    public Vector3 hoverStartPosition;    // Guarda posición al entrar
    private Vector3 targetHoverPosition;   // Posición a la que sube
    private float selecScale = 1.25f;
    private float hoverYOffset = 75f;
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private int indiceOriginal;

    private Coroutine currentAnimation;
    private bool isHovering = false;

    void Awake()
    {
        if (cardImage == null)
            cardImage = GetComponentInChildren<Image>();
        originalColor = cardImage.color;
        cardSelector = GetComponent<CardSelector>();

        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameManager.CurrentPhase != GamePhase.Preparation &&
            GameManager.CurrentPhase != GamePhase.Action)
        {
            if (!cardSelector.isSelected) cardImage.color = hoverColor;
            return;
        }

        if (isHovering) return;
        isHovering = true;

        // Guardar datos antes de animar
        hoverStartPosition = rectTransform.position;
        targetHoverPosition = hoverStartPosition + Vector3.up * hoverYOffset;
        indiceOriginal = transform.GetSiblingIndex();
        transform.SetAsLastSibling();

        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(Animate(hoverStartPosition, targetHoverPosition, originalScale, originalScale * selecScale));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (GameManager.CurrentPhase != GamePhase.Preparation &&
            GameManager.CurrentPhase != GamePhase.Action)
        {
            if (!cardSelector.isSelected) cardImage.color = originalColor;
            return;
        }

        if (!isHovering) return;
        isHovering = false;

        // Restaurar orden y animar de regreso
        transform.SetSiblingIndex(indiceOriginal);

        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(Animate(rectTransform.position, hoverStartPosition, rectTransform.localScale, originalScale));
    }

    private IEnumerator Animate(Vector3 fromPos, Vector3 toPos, Vector3 fromScale, Vector3 toScale)
    {
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = easeCurve.Evaluate(elapsed / animationDuration);
            rectTransform.position = Vector3.Lerp(fromPos, toPos, t);
            rectTransform.localScale = Vector3.Lerp(fromScale, toScale, t);
            yield return null;
        }
        rectTransform.position = toPos;
        rectTransform.localScale = toScale;
        currentAnimation = null;
    }
}
