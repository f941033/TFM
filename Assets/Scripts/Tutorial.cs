using System;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public CardManager cardManager;
    public GameManager gameManager;

    List<CardData> startingDeck = new List<CardData>();
    int startingNumberOfCardsInDeck = 10;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
            if(card.cardName == "Foso" || card.cardName == "Pinchos" || card.cardName == "Bola Gigante" || card.cardName == "Ácido corrosivo" || card.cardName == "Sierra giratoria")
            {
                startingDeck.Add(card);
            }

            if (card.cardName == "MasRapido" || card.cardName == "Muerteeeee" || card.cardName == "PegaLeches10000" || card.cardName == "Reciclado" || card.cardName == "Doble robo")
            {
                startingDeck.Add(card);
            }
        }
    }

    public void PlayButton()
    {
        cardManager.PreparationPhase(startingDeck);
        gameManager.SetSelectedCards(startingDeck);
        gameObject.SetActive(false);
    }


}
