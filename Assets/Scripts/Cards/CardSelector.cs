using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardSelector : MonoBehaviour, IPointerClickHandler
{
    public Image cardImage; // Asigna el componente Image de la carta en el inspector
    public Color selectedColor = Color.green;
    public Color deselectedColor = Color.white;
    [HideInInspector] public bool isSelected = false;

    private CardSelectionManager selectionManager;
    private CardDragDrop dataHolder; // Script que referencia el ScriptableObject cardData ¿cómo lo obtengo directamente?
    public TextMeshProUGUI textName;
    void Awake()
    {
        selectionManager = FindAnyObjectByType<CardSelectionManager>();
        dataHolder = GetComponent<CardDragDrop>();
        cardImage.color = deselectedColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        selectionManager.SelectCard(this);
    }

    public void Select()
    {
        isSelected = true;
        cardImage.color = selectedColor;
    }

    public void Deselect()
    {
        isSelected = false;
        cardImage.color = deselectedColor;
    }

    public CardData GetCardData()
    {
        return dataHolder.cardData;
    }

    public string GetCardName()
    {
        return textName.text;
    }
}
