using DeckboundDungeon.GamePhase;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardHoverInHand : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuración")]
    public Color hoverColor = new Color(1f, 0.9f, 0.6f, 1f); // Color al hacer hover 
    private Color originalColor;

    [Header("Referencias")]
    public Image cardImage; // Asigna el componente Image de la carta
    private CardSelector cardSelector;

    // --------- NUEVO HOVER EN ESCALA -------------  //
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private float selecScale = 1.25f;
    private int indiceOriginal;
    // ----------------------------------------------- //

    void Awake()
    {
        if (cardImage == null)
            cardImage = GetComponentInChildren<Image>();
        originalColor = cardImage.color;
        cardSelector = GetComponent<CardSelector>();

        // --------- NUEVO HOVER EN ESCALA -------------  //
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        // ----------------------------------------------- //

    }

    public void OnPointerEnter(PointerEventData eventData)
    {

        // --------- NUEVO HOVER EN ESCALA -------------  //
        if (GameManager.CurrentPhase == GamePhase.Preparation || GameManager.CurrentPhase == GamePhase.Action)
        {
            indiceOriginal = transform.GetSiblingIndex();
            transform.SetAsLastSibling();
            rectTransform.localScale = originalScale * selecScale;
            rectTransform.position += new Vector3(0f, 1f);
        }
        // ----------------------------------------------- //
        else
        {
            if (!cardSelector.isSelected)
                cardImage.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // --------- NUEVO HOVER EN ESCALA -------------  //
        if (GameManager.CurrentPhase == GamePhase.Preparation || GameManager.CurrentPhase == GamePhase.Action)
        {
            transform.SetSiblingIndex(indiceOriginal);
            rectTransform.localScale = originalScale;
        }
        // ----------------------------------------------- //
        else
        {
            if (!cardSelector.isSelected)
            cardImage.color = originalColor;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

}
