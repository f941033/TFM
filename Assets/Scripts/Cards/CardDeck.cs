using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardDeck : MonoBehaviour, IPointerClickHandler
{
    public GameSettings settings;
    [Header("Referencias")]
    [SerializeField] public GameObject selectionBorder; // Borde hijo de la carta
    [SerializeField] public CardData cardData; // ScriptableObject con datos de la carta
    [SerializeField] private TextMeshProUGUI textName;
    [SerializeField] private TextMeshProUGUI textDesc;
    [SerializeField] private TextMeshProUGUI textCost; 
    [Header("Configuraci�n")]
    [SerializeField] public CardSummaryUI summaryPanel; // Panel de resumen
    public bool isSelectable = true;
    private bool isSelected = false;
    private int maxCardDeck => settings.initialDeckSize;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isSelectable) return;
        ToggleSelection();
        UpdateSummaryPanel();
    }

    private void ToggleSelection()
    {
        if (isSelected || (!isSelected && (summaryPanel.selectedCards.Count < maxCardDeck)))
        {
            isSelected = !isSelected;
            selectionBorder.SetActive(isSelected);
        }
    }

    private void UpdateSummaryPanel()
    {
        if (isSelected)
        {
            summaryPanel.SelectCard(cardData);
        }
        else
        {
            summaryPanel.DeselectCard(cardData);
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Inicializa el borde como desactivado
        selectionBorder.SetActive(false);
        textName.text = cardData.cardName;
        textDesc.text = cardData.description;
        textCost.text = cardData.cost.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
