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
    public List<GameObject> cardsInHand = new List<GameObject>();
    public byte handSize = 4;
    public byte currentHandSize;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
        //currentHandSize = handSize;
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
        cardUI.SetCardUI(cardToDraw);
        if (cardToDraw.cardType == CardType.Trap || cardToDraw.cardType == CardType.DeckEffect)
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
            if (drawPile.Count == 0 && discardPile.Count > 0)
            {
                // Mezclar descartes y pasarlos al drawPile
                drawPile.AddRange(discardPile);
                Shuffle(drawPile);
                discardPile.Clear();
                gameManager.DeactivateDiscardPileImage();
                FindFirstObjectByType<GameManager>().UpdateTextNumberOfCardsDiscard();
            }

            if (drawPile.Count > 0)
            {
                DrawCard();
            }
            else
            {
                break; // no se puede robar más
            }
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

            var cardObj = Instantiate(prefabCard, panelCard);
            var cardUI = cardObj.GetComponentInChildren<CardUI>();
            cardUI.SetCardUI(cardData);

            Debug.Log("la carta cogida de descartes es: " + cardData.cardName);
            if (cardData.cardType == CardType.Trap || cardData.cardType == CardType.DeckEffect)
            {
                var drag = cardObj.GetComponent<CardDragDrop>();
                drag.dropTilemap = zonaValidaTilemap;
                drag.cardData = cardData;
                drag.player = player;
                drag.Deck = this;
            }
            else if (cardData is BuffCardData buffData)
            {
                Destroy(cardObj.GetComponent<CardDragDrop>());
                var hability = cardObj.GetComponentInChildren<HabilityCardHandler>(includeInactive: true);
                hability.Initialize(buffData, player);
            }

            cardsInHand.Add(cardObj);
            //DrawCard();
            count--;
        }
    }

    public void DiscardHand()
    {
        // suponemos que cardsInHand es la lista privada de GameObject de cartas
        foreach (var cardGO in cardsInHand)
        {
            // sacamos el CardData de cada carta
            var ui = cardGO.GetComponentInChildren<CardUI>();
            if (ui != null && ui.data != null)
                discardPile.Add(ui.data);

            Destroy(cardGO);
        }
        cardsInHand.Clear();

        // avisa al GameManager para actualizar UI de pila de descarte, si quieres
        FindFirstObjectByType<GameManager>().UpdateTextNumberOfCardsDiscard();

        if (discardPile.Count > 0)
            gameManager.ActivateDiscardPileImage();
    }
}
