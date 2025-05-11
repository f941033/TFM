using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using DeckboundDungeon.Cards.Buff;
using TMPro;

public class CardManager : MonoBehaviour
{
    //private List<Card> cardInHand = new List<Card>();
    public GameObject prefabCard;
    public Transform panelCard;
    public Tilemap zonaValidaTilemap;
    private PlayerController player;
    public TextMeshProUGUI textNumberOfCardsDeck;
    public GameObject drawPileImage;

    [Header("Variables del mazo")]
    public List<CardData> startingDeck;
    public List<CardData> drawPile = new List<CardData>();
    public List<CardData> discardPile = new List<CardData>();
    private List<GameObject> cardsInHand   = new List<GameObject>();
    public byte handSize = 3;
    public byte currentHandSize;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
        currentHandSize = handSize;
    }

    /*
    public void PreparationPhase(List<CardData> selectedCards)
    {
        startingDeck = new List<CardData>(selectedCards);

        //Crear el mazo de cartas de tipo trampa
        foreach (CardData card in startingDeck) 
        { 
            if(card.cardType == CardType.Trap)
            {
                drawPile.Add(card);
            }
        }
       
        DrawFullHand();
    }
    */

    public void StartRun(List<CardData> selectedCards)
    {
        startingDeck = new List<CardData>(selectedCards);
        drawPile = new List<CardData>(startingDeck);
        discardPile.Clear();
        Debug.Log(startingDeck.Count);
        textNumberOfCardsDeck.text = drawPile.Count.ToString();
        Shuffle(drawPile);
        DrawFullHand();
        Time.timeScale = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DrawCard() {
        if (drawPile.Count == 0) {
            drawPile.AddRange(discardPile);
            discardPile.Clear();
            Shuffle(drawPile);
            drawPileImage.SetActive(true);
        }
        var cardToDraw = drawPile[0];
        drawPile.RemoveAt(0);

        if (drawPile.Count == 0)
        {
            drawPileImage.SetActive(false);
        }
        else
        {
            textNumberOfCardsDeck.text = drawPile.Count.ToString();
        }
        

        var cardData = Instantiate(prefabCard, panelCard);
        CardUI cardUI = cardData.GetComponentInChildren<CardUI>();
        cardUI.setCardUI(cardToDraw);

        var drag = cardData.GetComponent<CardDragDrop>();
        drag.dropTilemap = zonaValidaTilemap;
        drag.cardData = cardToDraw;
        drag.player = player;
        drag.Deck = this;

        cardsInHand.Add(cardData);
    }

    public void DrawFullHand(){
        while(cardsInHand.Count < currentHandSize && (drawPile.Count > 0 || discardPile.Count > 0)){
            DrawCard();
        }
    }

    public void CardPlayed(GameObject card, CardData cardData){
        for(int i=0; i<discardPile.Count; i++){
            
        }
        cardsInHand.Remove(card);
        discardPile.Add(cardData);
        Destroy(card);
        if (cardData.cardType != CardType.DeckEffect)
        {
            DrawFullHand();
        }
    }

    public void Shuffle<T>(List<T> list){
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
