using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardSummaryUI : MonoBehaviour
{
    public GameSettings settings;
    public GameObject cardNamePrefab; // Prefab UI con componente Text
    public TextMeshProUGUI contador;
    public Button botonListo;
    public GameObject canvasDeck;
    public CardManager cardManager;
    public Transform listContainer;

    public List<CardData> selectedCards = new List<CardData>();
    private int maxCardDeck => settings.initialDeckSize;

    private void Start()
    {
        Time.timeScale = 0f;
        contador.text = selectedCards.Count + "/" + maxCardDeck;
    }

    public void SelectCard(CardData card)
    {
        if (!selectedCards.Contains(card))
        {
            selectedCards.Add(card);
            UpdateSummaryPanel();
        }
    }

    public void DeselectCard(CardData card)
    {
        if (selectedCards.Contains(card))
        {
            selectedCards.Remove(card);
            UpdateSummaryPanel();
        }
    }

    private void UpdateSummaryPanel()
    {
        // Limpiar el panel
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // A�adir un texto por cada carta seleccionada
        foreach (var card in selectedCards)
        {
            GameObject go = Instantiate(cardNamePrefab, transform);
            Text text = go.GetComponent<Text>();
            text.text = card.cardName;
        }

        contador.text = selectedCards.Count + "/" + maxCardDeck;
        if (selectedCards.Count == maxCardDeck)
        {
            botonListo.interactable = true;
        }
        else
        {
            botonListo.interactable = false;
        }
    }

    public void BotonListo()
    {
        cardManager.StartRun(selectedCards);

        foreach (Transform child in listContainer) 
            Destroy(child.gameObject);
        selectedCards.Clear();

        canvasDeck.SetActive(false);
        Time.timeScale = 1f;
    }
}
