using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardSummaryUI : MonoBehaviour
{
    public GameSettings settings;
    public GameObject cardNamePrefab; // Prefab UI con componente Text
    public TextMeshProUGUI counterText;
    public TextMeshProUGUI baseHealthText;
    public Button readyButton;
    public GameObject canvasDeck;
    public CardManager cardManager;
    public Transform listContainer;
    public PlayerController playerController;
    public GameManager gameManager;

    public List<CardData> selectedCards = new List<CardData>();
    private int maxCardDeck => settings.initialDeckSize;
    public float baseHealth;

    private void Start()
    {
        counterText.text = selectedCards.Count + "/" + maxCardDeck;
        GameObject.Find("Main Camera").GetComponent<CameraMovement>().SendMessage("StopCameraMovement");
        baseHealth = playerController.BaseHealth;
        baseHealthText.text = "Salud base: " + baseHealth;
    }

    public void SelectCard(CardData card)
    {
        if (!selectedCards.Contains(card))
        {
            selectedCards.Add(card);
            baseHealth += card.healthModifier;
            UpdateSummaryPanel();
        }
    }

    public void DeselectCard(CardData card)
    {
        if (selectedCards.Contains(card))
        {
            selectedCards.Remove(card);
            baseHealth -= card.healthModifier;
            UpdateSummaryPanel();
        }
    }

    private void UpdateSummaryPanel()
    {
        baseHealthText.text = "Salud base: " + baseHealth;

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

        counterText.text = selectedCards.Count + "/" + maxCardDeck;
        if (selectedCards.Count == maxCardDeck)
        {
            readyButton.interactable = true;
        }
        else
        {
            readyButton.interactable = false;
        }
    }

    public void ReadyButton()
    {
        Time.timeScale = 1f;
        canvasDeck.SetActive(false);
        cardManager.PreparationPhase(selectedCards);
        //playerController.BaseHealth = baseHealth;
        gameManager.SetSelectedCards(selectedCards);
        //cardManager.PreparationPhase(selectedCards);

        //foreach (Transform child in listContainer)
            //Destroy(child.gameObject);
        //selectedCards.Clear();

        //canvasDeck.SetActive(false);

    }

    public void ResetRun()
    {
        foreach (Transform child in listContainer)
            Destroy(child.gameObject);
        selectedCards.Clear();
        counterText.text = $"0/{settings.initialDeckSize}";
        baseHealth = playerController.BaseHealth;
        baseHealthText.text = "Salud base: " + baseHealth;
        readyButton.interactable = false;
    }
}
