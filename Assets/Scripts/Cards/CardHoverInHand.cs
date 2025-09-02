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
    public Vector3 basePosition; // NUEVA: Posición base fija que nunca cambia
    private Vector3 targetHoverPosition; // Posición a la que sube
    private float selecScale = 1.25f;
    private float hoverYOffset = 75f;
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private int indiceOriginal;

    private Coroutine currentAnimation;
    private bool isHovering = false;
    private bool isInitialized = false; // NUEVA: Para evitar problemas de inicialización

    void Awake()
    {
        if (cardImage == null)
            cardImage = GetComponentInChildren<Image>();
        originalColor = cardImage.color;
        cardSelector = GetComponent<CardSelector>();

        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    void Start()
    {
        // NUEVO: Guardar la posición base al inicio, cuando ya está posicionada correctamente
        StartCoroutine(InitializeBasePosition());
    }

    private IEnumerator InitializeBasePosition()
    {
        // Esperar un frame para que el layout esté completamente calculado
        yield return null;
        basePosition = rectTransform.position;
        isInitialized = true;
    }

    // NUEVO: Método público para actualizar la posición base cuando sea necesario
    public void UpdateBasePosition()
    {
        if (!isHovering && isInitialized)
        {
            basePosition = rectTransform.position;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInitialized) return; // Evitar problemas antes de la inicialización

        if (GameManager.CurrentPhase != GamePhase.Preparation &&
            GameManager.CurrentPhase != GamePhase.Action)
        {
            if (!cardSelector.isSelected) cardImage.color = hoverColor;
            return;
        }

        if (isHovering) return;
        isHovering = true;

        // CAMBIO CLAVE: Usar siempre basePosition en lugar de la posición actual
        targetHoverPosition = basePosition + Vector3.up * hoverYOffset;
        indiceOriginal = transform.GetSiblingIndex();
        transform.SetAsLastSibling();

        if (currentAnimation != null) StopCoroutine(currentAnimation);

        // Animar desde la posición actual (donde esté) hacia la posición hover
        currentAnimation = StartCoroutine(Animate(rectTransform.position, targetHoverPosition, rectTransform.localScale, originalScale * selecScale));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInitialized) return;

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

        // CAMBIO CLAVE: Volver siempre a basePosition, no a una posición variable
        currentAnimation = StartCoroutine(Animate(rectTransform.position, basePosition, rectTransform.localScale, originalScale));
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

        // Asegurar valores finales exactos
        rectTransform.position = toPos;
        rectTransform.localScale = toScale;
        currentAnimation = null;
    }

    // NUEVO: Método para forzar reseteo a posición base (útil para debugging o casos especiales)
    [ContextMenu("Reset to Base Position")]
    public void ResetToBasePosition()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }

        isHovering = false;
        rectTransform.position = basePosition;
        rectTransform.localScale = originalScale;
        transform.SetSiblingIndex(indiceOriginal);
    }

    // NUEVO: Método para visualizar la posición base en el editor (debugging)
    void OnDrawGizmosSelected()
    {
        if (isInitialized)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(basePosition, 0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(basePosition + Vector3.up * hoverYOffset, 0.1f);
        }
    }
}