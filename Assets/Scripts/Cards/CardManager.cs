using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using DeckboundDungeon.Cards.Buff;
using TMPro;
using System.Collections;

public class CardManager : MonoBehaviour
{
    //private List<Card> cardInHand = new List<Card>();
    public GameObject prefabCard;
    public Transform panelCard;
    public Tilemap zonaValidaTilemap;
    private PlayerController player;
    public TextMeshProUGUI textNumberOfCardsDeck;
    public TextMeshProUGUI textNumberOfCardsDiscardPile;
    public TextMeshProUGUI textCountDown;
    private int countDown = 20;
    public GameObject drawPileImage;
    public GameObject discardPileImage;
    public AudioManager audioManager;

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

    public void ActivateDiscardPileImage()
    {
        discardPileImage.SetActive(true);
    }

    public void UpdateTextNumberOfCardsDiscard()
    {
        textNumberOfCardsDiscardPile.text = discardPile.Count.ToString();
    }

    //----------------------------------------------------------
    //                   FASE DE PREPARACI�N
    //----------------------------------------------------------
    public void PreparationPhase(List<CardData> selectedCards)
    {
        startingDeck = new List<CardData>(selectedCards);
        
        //Crear el mazo de cartas de tipo TRAMPA
        foreach (CardData card in startingDeck) 
        { 
            if(card.cardType == CardType.Trap)
            {
                drawPile.Add(card);
            }
        }

        StartCoroutine("CountDown");


        discardPile.Clear();
        discardPileImage.SetActive(false);
        Debug.Log(startingDeck.Count);
        textNumberOfCardsDeck.text = drawPile.Count.ToString();

        Shuffle(drawPile);
        DrawFullHand();
    }
 
    IEnumerator CountDown()
    {
        while (countDown > 0)
        {
            yield return new WaitForSeconds(1);
            countDown--;
            textCountDown.text = countDown.ToString();
            if (countDown <= 3) audioManager.SoundBeep();
        }
        StartRun();        
    }


    //----------------------------------------------------------
    //                   INICIO FASE DE ACCI�N
    //----------------------------------------------------------
    public void StartRun()
    {
        //startingDeck = new List<CardData>(selectedCards);
        textCountDown.gameObject.SetActive(false);
        ClearPanelCard();
        drawPile = new List<CardData>();

        drawPileImage.SetActive(true);


        //Crear el mazo de cartas de tipo ACCI�N
        foreach (CardData card in startingDeck)
        {
            if (card.cardType != CardType.Trap)
            {
                drawPile.Add(card);
            }
        }

        discardPile.Clear();
        discardPileImage.SetActive(false);
        Debug.Log(startingDeck.Count);
        textNumberOfCardsDeck.text = drawPile.Count.ToString();
        
        Shuffle(drawPile);
        DrawFullHand();
        GameObject.Find("SpawnManager").GetComponent<SpawnEnemies>().SendMessage("GenerarEnemigos");
        //GameObject.Find("Main Camera").GetComponent<CameraMovement>().SendMessage("StartCameraMovement");
    }


    void ClearPanelCard()
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

    public void DrawCard() {
        if (drawPile.Count == 0) {
            drawPile.AddRange(discardPile);
            discardPile.Clear();
            discardPileImage.SetActive(false);
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
