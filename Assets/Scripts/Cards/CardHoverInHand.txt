using DeckboundDungeon.GamePhase;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class CardHoverInHand : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Referencias")]
    public Image haloImage;
    private CardSelector cardSelector;

    // Hover en escala y animación
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Quaternion originalRotation; // Nueva variable para guardar la rotación original
    public Vector3 basePosition;
    private Vector3 targetHoverPosition;
    private float selecScale = 1.25f;
    private float hoverYOffset = 120f;
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private int indiceOriginal;

    private Coroutine currentAnimation;
    private bool isHovering = false;
    private bool isInitialized = false;

    void Awake()
    {
        cardSelector = GetComponent<CardSelector>();
        haloImage.gameObject.SetActive(false);
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        originalRotation = rectTransform.localRotation; // Guardar rotación original
    }

    void Start()
    {
        StartCoroutine(InitializeBasePosition());
    }

    private IEnumerator InitializeBasePosition()
    {
        yield return null;
        basePosition = rectTransform.localPosition;
        targetHoverPosition = basePosition + Vector3.up * hoverYOffset;
        originalRotation = rectTransform.localRotation; // Actualizar después del layout
        isInitialized = true;
    }


    public void ForceUpdateBasePosition()
    {
        if (isInitialized)
        {
            Vector3 oldBase = basePosition;
            basePosition = rectTransform.localPosition;
            targetHoverPosition = basePosition + Vector3.up * hoverYOffset;
            originalRotation = rectTransform.localRotation; // Actualizar rotación base
            indiceOriginal = transform.GetSiblingIndex();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInitialized) return;

        if (GameManager.CurrentPhase != GamePhase.Preparation &&
            GameManager.CurrentPhase != GamePhase.Action)
        {
            if (!cardSelector.isSelected)
            {
                Color c = haloImage.color;
                c.a = 0.2f;
                haloImage.color = c;
                haloImage.gameObject.SetActive(true);
            }
            return;
        }

        if (isHovering) return;
        isHovering = true;

        targetHoverPosition = basePosition + Vector3.up * hoverYOffset;
        indiceOriginal = transform.GetSiblingIndex();
        transform.SetAsLastSibling();

        if (currentAnimation != null) StopCoroutine(currentAnimation);

        // Animar posición, escala Y rotación (enderezar la carta)
        currentAnimation = StartCoroutine(AnimateWithRotation(
            basePosition,
            targetHoverPosition,
            rectTransform.localScale,
            originalScale * selecScale,
            rectTransform.localRotation,
            Quaternion.identity // Rotación "recta" (sin inclinación)
        ));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInitialized) return;

        if (GameManager.CurrentPhase != GamePhase.Preparation &&
            GameManager.CurrentPhase != GamePhase.Action)
        {
            if (!cardSelector.isSelected)
                haloImage.gameObject.SetActive(false);

            return;
        }

        if (!isHovering) return;
        isHovering = false;

        transform.SetSiblingIndex(indiceOriginal);

        if (currentAnimation != null) StopCoroutine(currentAnimation);

        // Si solo queda esta carta, fuerza a 0° en origen
        Quaternion targetRot = (transform.parent.childCount == 1)
            ? Quaternion.identity
            : originalRotation;


        // Volver a posición, escala y rotación originales
        currentAnimation = StartCoroutine(AnimateWithRotation(
            targetHoverPosition,
            basePosition,
            rectTransform.localScale,
            originalScale,
            rectTransform.localRotation,
            targetRot // Volver a la rotación original del abanico
        ));
    }

    // Nueva corrutina que incluye animación de rotación
    private IEnumerator AnimateWithRotation(Vector3 fromPos, Vector3 toPos, Vector3 fromScale, Vector3 toScale, Quaternion fromRotation, Quaternion toRotation)
    {
        float elapsed = 0f;

        // Guarda la X fija
        float fixedX = rectTransform.localPosition.x;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            float eased = easeCurve.Evaluate(t);

            // Sólo interpola la Y; X permanece fija
            float newY = Mathf.Lerp(fromPos.y, toPos.y, t);
            rectTransform.localPosition = new Vector3(fixedX, newY, fromPos.z);
            //rectTransform.localPosition = Vector3.Lerp(fromPos, toPos, eased);
            rectTransform.localScale = Vector3.Lerp(fromScale, toScale, eased);
            rectTransform.localRotation = Quaternion.Slerp(fromRotation, toRotation, eased);

            yield return null;
        }

        // Asegurar valores finales exactos
        rectTransform.localPosition = new Vector3(fixedX, toPos.y, fromPos.z); ;
        rectTransform.localScale = toScale;
        rectTransform.localRotation = toRotation;
        currentAnimation = null;
    }


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
        rectTransform.localRotation = originalRotation; // Restaurar rotación original
        transform.SetSiblingIndex(indiceOriginal);
    }

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
