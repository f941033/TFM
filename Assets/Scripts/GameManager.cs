using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using DeckboundDungeon.GamePhase;
using System;

public class GameManager : MonoBehaviour
{
    private int enemiesDied = 0;
    public int initialEnemiesToKill = 3;
    public int enemiesToKillInCurrentWave;
    public int numberWave = 1;
    private List<CardData> selectedCards = new List<CardData>();
    private CardData[] allCards;

    public bool inPrepPhase = false;
    public TextMeshProUGUI textNumberWave;
    public GameObject panelEndWave;
    public CardManager cardManager;
    public CardSummaryUI cardSummaryUI;
    public PlayerController playerController;
    public CardSelectionManager selectionManager;
    public Tilemap tilemap;
    public GameObject drawPileImage;
    public GameObject discardPileImage;
    public GameObject panelTimeScale;
    public GameObject prefabCard;
    public Transform panelCard;
    public GameObject soulsBar;
    public GameObject countDownObject;
    public List<CardData> startingDeck;
    public SpawnEnemies spawnEnemies;
    public GameObject panelGo;
    public TextMeshProUGUI textNumberOfCardsDeck;
    public TextMeshProUGUI textNumberOfCardsDiscardPile;
    public Transform panelCardEndWave;
    [SerializeField] private TextMeshProUGUI messageText;
    private Coroutine HideMessageCO;
    public static GamePhase CurrentPhase { get; private set; }
    public static event Action<GamePhase> OnPhaseChanged;
    [SerializeField] private MerchantUI merchantUI;
    [SerializeField] private MerchantItem keyItemAsset;        // arrástralo en el Inspector
    [SerializeField] private MerchantItem potionItemAsset;     // idem

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChangePhase(GamePhase.Preparation);
        textNumberWave.text = "Ronda: " + numberWave.ToString();
        enemiesToKillInCurrentWave = Mathf.CeilToInt(initialEnemiesToKill * Mathf.Pow(numberWave, 0.8f));
    }



    // Update is called once per frame
    void Update()
    {

    }

    public void EnemyKaputt()
    {
        enemiesDied++;
        Debug.Log("enemigo " + enemiesDied + " muerto");
        if (enemiesDied == enemiesToKillInCurrentWave)
        {
            enemiesDied = 0;
            Invoke("EndWave", 1f);
        }
    }

    void EndWave()
    {
        ChangePhase(GamePhase.Preparation);
        Time.timeScale = 1f;
        panelEndWave.SetActive(true);
        GenerateRewardCard();
    }


    void GenerateRewardCard()
    {
        foreach (Transform t in panelCardEndWave)
        {
            Destroy(t.gameObject);
        }
        allCards = Resources.LoadAll<CardData>("Cards");
        List<CardData> selectedCards = new List<CardData>();
        bool isSpecialRound = (numberWave % 5 == 0);
        CardData turretCard = null;

        // Buscar la carta de torreta
        foreach (CardData card in allCards)
        {
            if (card.cardName.Contains("Turrent"))
            {
                turretCard = card;
                break;
            }
        }

        if (isSpecialRound && turretCard != null)
        {
            // Añadir 1 carta de torreta y 2 aleatorias
            selectedCards.Add(turretCard);

            // Generar 2 cartas aleatorias que no sean la torreta
            for (int i = 0; i < 2; i++)
            {
                CardData randomCard;
                do
                {
                    randomCard = allCards[UnityEngine.Random.Range(0, allCards.Length)];
                } while (randomCard == turretCard);

                selectedCards.Add(randomCard);
            }

        }
        else
        {
            // Cartas completamente aleatorias
            for (int i = 0; i < 3; i++)
            {
                selectedCards.Add(allCards[UnityEngine.Random.Range(0, allCards.Length)]);
            }
        }

        // Instanciar las cartas seleccionadas
        foreach (CardData card in selectedCards)
        {
            var cardData = Instantiate(prefabCard, panelCard);
            CardUI cardUI = cardData.GetComponentInChildren<CardUI>();
            cardUI.setCardUI(card);
            Destroy(cardData.GetComponent<CardDragDrop>());
        }
    }

    public void PlayAnotherRun()
    {
        Debug.Log("vamos a por otra ronda");

        messageText.gameObject.SetActive(false);
        cardManager.ClearPanelCard();
        panelEndWave.SetActive(false);
        if (numberWave % 5 == 0)
        {
            MerchantShop();
        }
        numberWave++;
        textNumberWave.text = "Ronda: " + numberWave;
        enemiesToKillInCurrentWave = Mathf.CeilToInt(initialEnemiesToKill * Mathf.Pow(numberWave, 0.8f));


        /*------------------------------------------------
         *           A�ADIR CARTA NUEVA AL DECK
         *------------------------------------------------   */

        //esto es una chapuza pero funciona:

        string selectedName = selectionManager.GetSelectedCardName();
        allCards = Resources.LoadAll<CardData>("Cards");

        foreach (var item in allCards)
        {
            if (item.cardName == selectedName)
                selectedCards.Add(item);
        }

        PreparationPhase(selectedCards);
    }

    public void ChangePhase(GamePhase newPhase)
    {
        CurrentPhase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);
    }

    public void SetSelectedCards(List<CardData> selectedCards)
    {
        this.selectedCards = selectedCards;
    }

    public void ShowMessage(string text, float duration)
    {
        if (HideMessageCO != null)
            StopCoroutine(HideMessageCO);
        messageText.text = text;
        messageText.gameObject.SetActive(true);
        HideMessageCO = StartCoroutine(HideMessage(duration));
    }

    private IEnumerator HideMessage(float duration)
    {
        yield return new WaitForSeconds(duration);
        messageText.gameObject.SetActive(false);
        messageText.text = "";
        HideMessageCO = null;
    }

    //----------------------------------------------------------
    //                   FASE DE PREPARACION
    //----------------------------------------------------------
    public void PreparationPhase(List<CardData> selectedCards)
    {
        inPrepPhase = true;
        ChangePhase(GamePhase.Preparation);
        panelTimeScale.SetActive(false);
        drawPileImage.SetActive(true);
        countDownObject.SetActive(true);
        cardManager.ClearPanelCard();
        soulsBar.SetActive(true);
        playerController.RefillSouls();
        //GameObject.Find("Main Camera").GetComponent<CameraMovement>().SendMessage("StartCameraMovement");

        Debug.Log("vida base del player: " + FindFirstObjectByType<PlayerController>().baseHealth.ToString());
        startingDeck = new List<CardData>(selectedCards);

        //Crear el mazo de cartas de tipo TRAMPA
        foreach (CardData card in startingDeck)
        {
            if (card.cardType == CardType.Trap)
            {
                cardManager.drawPile.Add(card);
            }
        }

        cardManager.discardPile.Clear();
        discardPileImage.SetActive(false);
        Debug.Log(startingDeck.Count);
        textNumberOfCardsDeck.text = startingDeck.Count.ToString();

        cardManager.Shuffle(cardManager.drawPile);
        cardManager.Shuffle(startingDeck);
        cardManager.DrawFullHand();

        // ---------------PUNTOS DE SPAWN DE ENEMIGOS-----------------
        spawnEnemies.DesactivarLuces();
        spawnEnemies.GenerarPuntosSpawn(numberWave);

    }

    //----------------------------------------------------------
    //                   INICIO FASE DE ACCION
    //----------------------------------------------------------
    public void StartRun()
    {
        inPrepPhase = false;
        ChangePhase(GamePhase.Action);
        countDownObject.SetActive(false);
        panelTimeScale.SetActive(true);
        StartCoroutine("PanelGo");

        EliminarCanvasEnemies();

        cardManager.ClearPanelCard();
        cardManager.drawPile = new List<CardData>();

        drawPileImage.SetActive(true);

        soulsBar.SetActive(false);

        //Crear el mazo de cartas de tipo ACCI�N
        foreach (CardData card in startingDeck)
        {
            if (card.cardType != CardType.Trap)
            {
                cardManager.drawPile.Add(card);
            }
        }

        cardManager.discardPile.Clear();
        discardPileImage.SetActive(false);
        Debug.Log(startingDeck.Count);
        textNumberOfCardsDeck.text = cardManager.drawPile.Count.ToString();

        cardManager.Shuffle(cardManager.drawPile);
        cardManager.DrawFullHand();

        var spawner = GameObject.Find("SpawnManager").GetComponent<SpawnEnemies>();
        spawner.StartCoroutine(spawner.GenerarEnemigos());
    }

    void EliminarCanvasEnemies()
    {
        GameObject[] canvasEnemies = GameObject.FindGameObjectsWithTag("CanvasEnemies");
        foreach (var item in canvasEnemies)
        {
            Destroy(item);
        }
    }

    public void ActivateDiscardPileImage()
    {
        discardPileImage.SetActive(true);
    }
    public void UpdateTextNumberOfCardsDiscard()
    {
        textNumberOfCardsDiscardPile.text = cardManager.discardPile.Count.ToString();
    }
    public void SkipCountDown()
    {
        StartRun();
    }
    IEnumerator PanelGo()
    {
        panelGo.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        panelGo.SetActive(false);
    }

    void MerchantShop()
    {
        var shopList = new List<MerchantItem>();
        // 1 llave
        shopList.Add(keyItemAsset);
        // 1 poción
        shopList.Add(potionItemAsset);

        // 2 cartas aleatorias de tipo Trap o Buff
        var all = Resources.LoadAll<CardData>("Cards");
        var pool = new List<CardData>();
        foreach (var cards in all) if (cards.cardType == CardType.Trap || cards.cardType == CardType.Buff) pool.Add(cards);
        for (int i = 0; i < 2; i++)
        {
            var pick = pool[UnityEngine.Random.Range(0, pool.Count)];
            var cardItem = ScriptableObject.CreateInstance<CardItem>();
            cardItem.itemName = pick.cardName;
            //cardItem.icon     = /* tu icono */;
            cardItem.cost = pick.goldCost;
            cardItem.cardData = pick;
            shopList.Add(cardItem);
        }

        // 2 cartas “deck effect” (deck cards)
        pool.Clear();

        foreach (var cards in all) if (cards.cardType == CardType.DeckEffect) pool.Add(cards);
        for (int i = 0; i < 2; i++)
        {
            var pick = pool[UnityEngine.Random.Range(0, pool.Count)];
            var cardItem = ScriptableObject.CreateInstance<CardItem>();
            cardItem.itemName = pick.cardName;
            //cardItem.icon     = /* tu icono */;
            cardItem.cost = pick.goldCost;
            cardItem.cardData = pick;
            shopList.Add(cardItem);
        }

        merchantUI.Show(shopList);
        return;
    }

    public void PreparationPhase()
    {
        PreparationPhase(selectedCards);
    }

    public void AddCardToDeck(CardData card)
    {
        selectedCards.Add(card);
    }
}
