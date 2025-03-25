using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class CardManager : MonoBehaviour
{
    private List<Card> cardInHand = new List<Card>();
    public GameObject prefabCard;
    public Transform panelCard;
    public Tilemap zonaValidaTilemap;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        addCardToHand(new Card("Pinchos", 0, "Carta de pinchos que pincha y hace pupa", "Trampa", "Común", 0, 0, 0));
        addCardToHand(new Card("Foso", 0, "Agujero por el que se cae y hace pupa", "Trampa", "Común", 0, 0, 0));
        addCardToHand(new Card("Bomba", 0, "Cosa que explota y hace mucha pupa", "Trampa", "Común", 0, 0, 0));
        addCardToHand(new Card("Invoca monstruos", 0, "Invoca un perrete que ataca", "Trampa", "Común", 0, 0, 0));
        addCardToHand(new Card("Upgrade ataque", 0, "durante 10 seg el villano hace más pupa", "Trampa", "Común", 0, 0, 0));
    }
    public void addCardToHand(Card card){

        GameObject newCard = Instantiate(prefabCard, panelCard);
        CardUI cardUI = newCard.GetComponentInChildren<CardUI>();
        cardUI.setCardUI(card);

        cardInHand.Add(card);
        newCard.GetComponent<CardDragDrop>().dropTilemap = zonaValidaTilemap;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
