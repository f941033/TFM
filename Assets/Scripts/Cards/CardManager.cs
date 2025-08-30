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
    public Transform handTransform; //root of the hand position
    public Tilemap zonaValidaTilemap;
    private PlayerController player;
    public GameManager gameManager;

    [Header("Variables de la mano")]
    float fanSpread = 7.5f;
    float cardSpacing = -150f;
    float verticalSpacing = 100f;


    [Header("Variables del mazo")]
    public List<CardData> drawPile = new List<CardData>();
    public List<CardData> discardPile = new List<CardData>();
    public List<GameObject> cardsInHand = new List<GameObject>();
    //public byte handSize = 4;
    public byte currentHandSize;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
        //currentHandSize = handSize;
    }

    public void ClearPanelCard()
    {
        foreach (Transform card in handTransform)
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
        if (drawPile.Count == 0 && discardPile.Count > 0)
        {
            // Mezclar descartes y pasarlos al drawPile
            drawPile.AddRange(discardPile);
            Shuffle(drawPile);
            discardPile.Clear();
            gameManager.DeactivateDiscardPileImage();
            FindFirstObjectByType<GameManager>().UpdateTextNumberOfCardsDiscard();
        }
        else if (drawPile.Count == 0 && discardPile.Count == 0)
        {
            gameManager.ShowMessage("No hay cartas que robar!", 2);
            return;
        }

        var cardToDraw = drawPile[0];
        drawPile.RemoveAt(0);


        gameManager.textNumberOfCardsDeck.text = drawPile.Count.ToString();

        // ------------- INSTANCIAR LA CARTA ------------- //
        var cardData = Instantiate(prefabCard, handTransform.position, Quaternion.identity, handTransform);
        CardUI cardUI = cardData.GetComponentInChildren<CardUI>();
        cardUI.SetCardUI(cardToDraw);

        if (cardToDraw.cardType == CardType.Trap || cardToDraw.cardType == CardType.DeckEffect || cardToDraw.cardType == CardType.Summon)
        {
            var drag = cardData.GetComponent<CardDragDrop>();
            drag.dropTilemap = zonaValidaTilemap;
            drag.cardData = cardToDraw;
            drag.player = player;
            drag.Deck = this;
        }
        else if (cardToDraw.cardType == CardType.Hability)
        {
            var drag = cardData.GetComponent<CardDragDrop>();
            drag.dropTilemap = zonaValidaTilemap;
            drag.cardData = cardToDraw;
            drag.player = player;
            drag.Deck = this;

            HabilityCardHandler hability = cardData.GetComponentInChildren<HabilityCardHandler>();
            hability.Initialize(cardToDraw, player);
        }
        else if (cardToDraw is BuffCardData buffData)
        {
            Destroy(cardData.GetComponent<CardDragDrop>());
            HabilityCardHandler hability = cardData.GetComponentInChildren<HabilityCardHandler>();
            hability.Initialize(buffData, player);
        }
        cardsInHand.Add(cardData);
        UpdateHandVisuals();
    }

    public void UpdateHandVisuals()
    {
        int cardCount = cardsInHand.Count;
        if (cardCount == 1)
        {
            cardsInHand[0].transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            cardsInHand[0].transform.localPosition = new Vector3(0f, 75f, 0f);
            return;
        }

        for (int i = 0; i < cardCount; i++)
        {
            float rotationAngle = (fanSpread * (i - (cardCount - 1) / 2f));
            cardsInHand[i].transform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);

            float horizontalOffset = (cardSpacing * (i - (cardCount - 1) / 2f));
            float normalizedPosition = (2f * i / (cardCount - 1) - 1f)/2f ; // Normalize card position between -1,1
            float verticalOffset = verticalSpacing * (1 - normalizedPosition * normalizedPosition );

            //set card position
            cardsInHand[i].transform.localPosition = new Vector3(horizontalOffset, verticalOffset,0f);

        }
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
        if (cardData.cardType != CardType.Hability)
        {
            cardsInHand.Remove(card);
            discardPile.Add(cardData);
            Destroy(card);
            UpdateHandVisuals();
        }

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

            var cardObj = Instantiate(prefabCard, handTransform.position, Quaternion.identity, handTransform);
            var cardUI = cardObj.GetComponentInChildren<CardUI>();
            cardUI.SetCardUI(cardData);

            Debug.Log("la carta cogida de descartes es: " + cardData.cardName);
            if (cardData.cardType == CardType.Trap || cardData.cardType == CardType.DeckEffect || cardData.cardType == CardType.Hability || cardData.cardType ==CardType.Summon)
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
                var hability = cardObj.GetComponentInChildren<HabilityCardHandler>();
                hability.Initialize(buffData, player);
            }

            cardsInHand.Add(cardObj);
            UpdateHandVisuals();
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
            UpdateHandVisuals();
        }
        cardsInHand.Clear();

        // avisa al GameManager para actualizar UI de pila de descarte, si quieres
        FindFirstObjectByType<GameManager>().UpdateTextNumberOfCardsDiscard();

        if (discardPile.Count > 0)
            gameManager.ActivateDiscardPileImage();
    }
}
