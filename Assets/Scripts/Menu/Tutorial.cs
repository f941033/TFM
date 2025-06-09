using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public CardManager cardManager;
    public GameManager gameManager;
    public TextMeshProUGUI healthText;
    public PlayerController playerController;

    List<CardData> startingDeck = new List<CardData>();
    float baseHealth;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        baseHealth = playerController.baseHealth;
        GenerateDeckPredef();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void GenerateDeckPredef()
    {
        var allCards = Resources.LoadAll<CardData>("Cards");
        /*
        for (int i = 0; i < startingNumberOfCardsInDeck; i++)
        {
            var index = Random.Range(0, allCards.Length);
            startingDeck.Add(allCards[index]);
        }*/

        foreach (var card in allCards)
        {
            if (card.cardName == "Foso" || card.cardName == "Pinchos" || card.cardName == "Torreta" || card.cardName == "Bomba" || card.cardName == "Sierra giratoria")
            {
                startingDeck.Add(card);
                baseHealth += card.healthModifier;

                if (card.cardName == "Pinchos" || card.cardName == "Muro" || card.cardName == "Bomba" || card.cardName == "Sierra giratoria")
                {
                    startingDeck.Add(card);
                    baseHealth += card.healthModifier;
                }
                if (card.cardName == "Pinchos" || card.cardName == "Bomba")
                {
                    startingDeck.Add(card);
                    baseHealth += card.healthModifier;
                }
            }

            if (card.cardName == "PegaLeches10000")
            {
                startingDeck.Add(card);
                baseHealth += card.healthModifier;

                //if (card.cardName == "MasRapido" || card.cardName == "Muerteeeee")
                //{
                //    startingDeck.Add(card);
                //}
            }


        }
    }

    public void PlayButton()
    {
        healthText.text = baseHealth.ToString();
        playerController.baseHealth = baseHealth;
        cardManager.PreparationPhase(startingDeck);
        gameManager.SetSelectedCards(startingDeck);
        gameObject.SetActive(false);
    }


}
