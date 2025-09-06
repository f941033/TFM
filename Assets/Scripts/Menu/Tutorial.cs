using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DeckboundDungeon.GamePhase;

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
        gameManager.ChangePhase(GamePhase.Tutorial);
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
            if (card.cardName == "DARK PIT" || card.cardName == "ACID POOL" || card.cardName == "FRAGILE BOX" || card.cardName == "BOOM!" || card.cardName == "THE BUTCHER")

            {
                startingDeck.Add(card);
                baseHealth += card.healthModifier;

                if (card.cardName == "FRAGILE BOX" || card.cardName == "BOOM!" || card.cardName == "THE BUTCHER")
                {
                    startingDeck.Add(card);
                    baseHealth += card.healthModifier;
                }
            }

            if (card.cardName == "DARK REAPER" || card.cardName == "RECYCLE")
            {
                startingDeck.Add(card);
                baseHealth += card.healthModifier;

            }
            if (card.cardName == "RAISE THE DEAD" || card.cardName == "RE-BOUNDED" || card.cardName == "FIREBALL" || card.cardName == "ZERO ZONE")
            {
                startingDeck.Add(card);
                baseHealth += card.healthModifier;

            }
            if(card.cardName == "GOBLIN")
            {
                startingDeck.Add(card);
                baseHealth += card.healthModifier;
            }

        }
    }

    public void PlayButton()
    {
        healthText.text = baseHealth.ToString();
        playerController.BaseHealth = baseHealth;
        gameManager.PreparationPhase(startingDeck);
        gameManager.SetSelectedCards(startingDeck);
        gameObject.SetActive(false);
    }


}
