using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardHoverInHand : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    [Header("Configuración")]   
    public Color hoverColor = new Color(1f, 0.9f, 0.6f, 1f); // Color al hacer hover 
    private Color originalColor;

    [Header("Referencias")]
    public Image cardImage; // Asigna el componente Image de la carta
    private CardSelector cardSelector;


    void Awake()
    {
        if (cardImage == null)
            cardImage = GetComponentInChildren<Image>();
        originalColor = cardImage.color;
        cardSelector = GetComponent<CardSelector>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!cardSelector.isSelected)
            cardImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!cardSelector.isSelected)
            cardImage.color = originalColor;
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
