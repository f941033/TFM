using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using DeckboundDungeon.Cards.Buff;

public class CardManager : MonoBehaviour
{
    //private List<Card> cardInHand = new List<Card>();
    public GameObject prefabCard;
    public Transform panelCard;
    public Tilemap zonaValidaTilemap;
    private PlayerController player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();

        CardData[] allCards = Resources.LoadAll<CardData>("Cards");
        foreach (var cardData in allCards){
            AddCardToHand(cardData);

        }

        /*addCardToHand(new Card("Pinchos", 0, "Carta de pinchos que pincha y hace pupa", "Trampa", "Común", 0, 0, 0));
        addCardToHand(new Card("Foso", 0, "Agujero por el que se cae y hace pupa", "Trampa", "Común", 0, 0, 0));
        addCardToHand(new Card("Bomba", 0, "Cosa que explota y hace mucha pupa", "Trampa", "Común", 0, 0, 0));
        addCardToHand(new Card("Invoca monstruos", 0, "Invoca un perrete que ataca", "Trampa", "Común", 0, 0, 0));
        addCardToHand(new Card("Upgrade ataque", 0, "durante 10 seg el villano hace más pupa", "Trampa", "Común", 0, 0, 0));*/
    }
    public void AddCardToHand(CardData card){

        GameObject newCard = Instantiate(prefabCard, panelCard);
        CardUI cardUI = newCard.GetComponentInChildren<CardUI>();
        cardUI.setCardUI(card);

        var drag = newCard.GetComponent<CardDragDrop>();
        drag.dropTilemap = zonaValidaTilemap;
        drag.cardData = card;
        drag.player = player;

        //cardInHand.Add(card);
        //newCard.GetComponent<CardDragDrop>().dropTilemap = zonaValidaTilemap;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
