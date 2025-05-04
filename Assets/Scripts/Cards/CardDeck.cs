using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardDeck : MonoBehaviour, IPointerClickHandler
{
    [Header("Referencias")]
    [SerializeField] private GameObject selectionBorder; // Borde hijo de la carta
    [SerializeField] private CardData cardData; // ScriptableObject con datos de la carta
    [SerializeField] private TextMeshProUGUI textName;
    [SerializeField] private TextMeshProUGUI textDesc;

    [Header("Configuración")]
    [SerializeField] private CardSummaryUI summaryPanel; // Panel de resumen

    private bool isSelected = false;
    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleSelection();
        UpdateSummaryPanel();
    }

    private void ToggleSelection()
    {
        isSelected = !isSelected;
        selectionBorder.SetActive(isSelected);
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
        textName.text = cardData.name;
        textDesc.text = cardData.description;

       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
