using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using DeckboundDungeon.Cards.Buff;
using TMPro;
using System.Collections;

public class CardManager : MonoBehaviour
{
    public GameObject prefabCard;
    public Transform panelCard;
    public Tilemap zonaValidaTilemap;
    private PlayerController player;
    public GameManager gameManager;

    [Header("Variables del mazo")]
    public List<CardData> drawPile = new List<CardData>();
    public List<CardData> discardPile = new List<CardData>();
    private List<GameObject> cardsInHand = new List<GameObject>();
    public byte handSize = 5;
    public byte currentHandSize;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
        currentHandSize = handSize;
    }

    public void ClearPanelCard()
    {
        foreach (Transform card in panelCard)
        {
            Destroy(card.gameObject);
        }
        cardsInHand.Clear();
    }



    // Update is called once per frame
    void Update()
    {

    }

    public void DrawCard()
    {
        var cardToDraw = drawPile[0];
        drawPile.RemoveAt(0);


        gameManager.textNumberOfCardsDeck.text = drawPile.Count.ToString();

        var cardData = Instantiate(prefabCard, panelCard);
        CardUI cardUI = cardData.GetComponentInChildren<CardUI>();
        cardUI.setCardUI(cardToDraw);
        if (cardToDraw.cardType == CardType.Trap)
        {
            var drag = cardData.GetComponent<CardDragDrop>();
            drag.dropTilemap = zonaValidaTilemap;
            drag.cardData = cardToDraw;
            drag.player = player;
            drag.Deck = this;
        }
        else if (cardToDraw is BuffCardData buffData)
        {
            Destroy(cardData.GetComponent<CardDragDrop>());
            HabilityCardHandler hability = cardData.GetComponentInChildren<HabilityCardHandler>(includeInactive: true);
            hability.Initialize(buffData, player);
        }
        cardsInHand.Add(cardData);
    }

    public void DrawFullHand()
    {

        while (cardsInHand.Count < currentHandSize && (drawPile.Count > 0 || discardPile.Count > 0))
        {
            DrawCard();
        }

    }

    public void CardPlayed(GameObject card, CardData cardData)
    {
        for (int i = 0; i < discardPile.Count; i++)
        {

        }
        cardsInHand.Remove(card);
        discardPile.Add(cardData);
        Destroy(card);

    }

    public void Shuffle<T>(List<T> list)
    {
        //Algoritmo de Fisher-Yates para que el shuffle sea siempre aleatorio
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    public void DrawFromDiscard(byte count)
    {
        while (count > 0 && discardPile.Count > 0)
        {
            CardData cardData = discardPile[discardPile.Count - 1];
            discardPile.RemoveAt(discardPile.Count - 1);

            drawPile.Insert(0, cardData);
            DrawCard();
            count--;
        }
    }
}
