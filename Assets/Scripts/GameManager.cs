using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using DeckboundDungeon.GamePhase;
using System;

public class GameManager : MonoBehaviour
{
    public GameObject audioSettingsPrefab;
    public GameObject panelKeyInfo;
    public CameraMovement cameraMovementInstance;
    private int enemiesDied = 0;
    public int initialEnemiesToKill = 3;
    public int enemiesToKillInCurrentWave;
    public int numberWave = 0;
    private List<CardData> selectedCards = new List<CardData>();
    private CardData[] allCards;
    private MerchantUI merchant;

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
    private MerchantUI merchantUI;
    [SerializeField] private MerchantItem keyItemAsset;
    [SerializeField] private MerchantItem potionItemAsset;
    [SerializeField] private MerchantItem soulsItemAsset;
    [SerializeField] private GameObject panelMerchant;
    [SerializeField] private int currentPrepHand = 1;
    [SerializeField] private const int totalPrepHands = 3;
    [SerializeField] private Button btnNextHand;
    [SerializeField] private TextMeshProUGUI prepHandCounterText;
    public GameObject handleNextHand;
    public bool hasKey = false;
    public TextMeshProUGUI nextHandText;
    [SerializeField] private Button skipButton;
    public GameObject runButton;
    private GamePhase previousPhase;
    public int closedRooms;
    [SerializeField] private GameObject mulliganPanel;
    [SerializeField] private Button mulliganButton;
    private bool spawnStarted = false;
    //private bool firstTimeBackgroundSound = true;
    private AudioSource audioSource;
    //public AudioClip clipBackgroundSound;
    private const string PREF_MULLIGAN_SEEN = "mulligan_seen_action";
    private const string PREF_INITIAL_TUTORIAL_SEEN = "initial_tutorial_seen";
    private bool endWave = false;
    public int keysBoughtThisRun = 0;
    [SerializeField] private GameObject CanvasInitialTutorial;

    private void Awake()
    {
        if (FindObjectsByType<AudioSettings>(FindObjectsSortMode.None).Length == 0)
            Instantiate(audioSettingsPrefab);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {  
        merchantUI = FindFirstObjectByType<MerchantUI>();
        ChangePhase(GamePhase.Preparation);
        skipButton.gameObject.SetActive(true);
        btnNextHand.onClick.AddListener(NextPreparationHand);
        //textNumberWave.text = "Ronda: " + numberWave.ToString();
        //enemiesToKillInCurrentWave = Mathf.CeilToInt(initialEnemiesToKill * Mathf.Pow(numberWave, 0.8f));
        merchant = FindAnyObjectByType<MerchantUI>();
        closedRooms = FindObjectsByType<SalaController>(FindObjectsSortMode.None).Length - 1;
        audioSource = GetComponent<AudioSource>();
        FindAnyObjectByType<AudioManager>().PlayBackgroundSound(0);
    }



    // Update is called once per frame
    void Update()
    {

    }

    public void EnemyKaputt()
    {
        StartCoroutine(DelayedEndWaveCheck());
    }

    private IEnumerator DelayedEndWaveCheck()
    {
        yield return new WaitForSecondsRealtime(2f);
        enemiesDied++;
        //if (enemiesDied >= enemiesToKillInCurrentWave)
        if (ZeroEnemies())
        {
            enemiesDied = 0;
            EndWave(); 
        }
    }

    private bool ZeroEnemies()
    {
        EnemyController[] allEnemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach (EnemyController enemy in allEnemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy && enemy.IsAlive())
            {
                return false; // Hay al menos un enemigo vivo
            }
        }
        return true;
    }

    void EndWave()
    {
        if (endWave) return;
        endWave = true;

        if (cardManager != null)
            cardManager.ForceCloseMulliganForEndWave();
        mulliganPanel.SetActive(false);
        ChangePhase(GamePhase.Preparation);
        Time.timeScale = 1f;
        panelEndWave.SetActive(true);
        cameraMovementInstance.EnableCameraControl(false);
        GenerateRewardCard();
    }


    void GenerateRewardCard()
    {
        foreach (Transform t in panelCardEndWave)
        {
            Destroy(t.gameObject);
        }
        ChangePhase(GamePhase.CardSelection);
        allCards = Resources.LoadAll<CardData>("Cards");
        var pool = new List<CardData>(allCards);

        //Buscamos las cartas que haya en el mazo inicial de tipo habilidad o bufo
        var ownedSpellNames = new HashSet<string>();
        if (startingDeck != null)
        {
            foreach (var c in startingDeck)
            {
                if (c == null) continue;
                if (c.cardType == CardType.Buff || c.cardType == CardType.Hability)
                    ownedSpellNames.Add(c.cardName);
            }
        }

        if (numberWave < 10)
            pool.RemoveAll(c => c.cardType == CardType.DeckEffect);

        pool.RemoveAll(c => (c.cardType == CardType.Buff || c.cardType == CardType.Hability) && ownedSpellNames.Contains(c.cardName));

        List<CardData> selectedCards = new List<CardData>();
        bool isSpecialRound = (numberWave > 0) && (numberWave % 5 == 0);

        CardData turretCard = pool.Find(c => c.cardName.Equals("ROCK-BOW"));

        if (isSpecialRound && turretCard != null)
        {
            selectedCards.Add(turretCard);
            pool.Remove(turretCard);
            // Generar 2 cartas aleatorias que no sean la torreta
            for (int i = 0; i < 2; i++)
            {
                int idx = UnityEngine.Random.Range(0, pool.Count);
                selectedCards.Add(pool[idx]);
                pool.RemoveAt(idx);
            }
        }
        else
        {
            pool.Remove(turretCard);
            for (int i = 0; i < 3; i++)
            {
                int idx = UnityEngine.Random.Range(0, pool.Count);
                selectedCards.Add(pool[idx]);
                pool.RemoveAt(idx);
            }
        }

        // Instanciar las cartas seleccionadas
        foreach (CardData card in selectedCards)
        {
            var cardData = Instantiate(prefabCard, panelCard);
            CardUI cardUI = cardData.GetComponentInChildren<CardUI>();
            cardUI.SetCardUI(card);
            Destroy(cardData.GetComponent<CardDragDrop>());
        }
    }

    public void PlayAnotherRun()
    {

        messageText.gameObject.SetActive(false);
        cardManager.ClearPanelCard();
        panelEndWave.SetActive(false);
        cameraMovementInstance.EnableCameraControl(true);

        string selectedName = selectionManager.GetSelectedCardName();
        allCards = Resources.LoadAll<CardData>("Cards");

        foreach (var item in allCards)
        {
            if (item.cardName == selectedName)
                selectedCards.Add(item);
        }

        if (numberWave % 5 == 0)
        {
            MerchantShop();
        }
        else
        {
            PreparationPhase();
        }
    }

    public void ChangePhase(GamePhase newPhase)
    {
        previousPhase = CurrentPhase;
        CurrentPhase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);
    }

    public void RestorePreviousPhase()
    {
        ChangePhase(previousPhase);
    }

    public void SetSelectedCards(List<CardData> selectedCards)
    {
        this.selectedCards = selectedCards;
    }

    public void ShowMessage(string text, float duration)
    {
        if (CurrentPhase == GamePhase.Merchant && merchantUI != null)
        {
            merchantUI.Speak(text, duration);
            return;
        }
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
        cardManager.ClearLastCard();
        bool firstTime = !Prefs.GetBool(PREF_INITIAL_TUTORIAL_SEEN, false);
        if (numberWave<1)
        {
            CanvasInitialTutorial.SetActive(true);
            Prefs.SetBool(PREF_INITIAL_TUTORIAL_SEEN, true);
        }

        mulliganButton.gameObject.SetActive(false);
        FindAnyObjectByType<AudioManager>().PlayBackgroundSound(1);

        cardManager.currentHandSize = 3;
        currentPrepHand = 1;
        UpdatePrepHandUI();
        merchant.DeactiveKey();
        runButton.SetActive(true); //mostrar run
        btnNextHand.transform.parent.gameObject.SetActive(true);
        soulsBar.SetActive(false);
        inPrepPhase = true;
        ChangePhase(GamePhase.Preparation);

        numberWave++;
        textNumberWave.text = "WAVE " + numberWave;
        enemiesToKillInCurrentWave = Mathf.CeilToInt(initialEnemiesToKill * Mathf.Pow(numberWave, 0.8f));

        panelTimeScale.SetActive(false);
        drawPileImage.SetActive(true);
        //countDownObject.SetActive(true);
        cardManager.ClearPanelCard();
        soulsBar.SetActive(true);
        playerController.RefillSouls();
        cardManager.drawPile.Clear();
        //GameObject.Find("Main Camera").GetComponent<CameraMovement>().SendMessage("StartCameraMovement");

        startingDeck = new List<CardData>(selectedCards);

        //Crear el mazo de cartas de tipo TRAMPA
        foreach (CardData card in startingDeck)
        {
            if (card.cardType == CardType.Trap || card.cardType == CardType.DeckEffect || card.cardType == CardType.Summon)
            {
                cardManager.drawPile.Add(card);
            }
        }

        cardManager.discardPile.Clear();
        discardPileImage.SetActive(false);
        textNumberOfCardsDeck.text = startingDeck.Count.ToString();

        cardManager.Shuffle(cardManager.drawPile);
        cardManager.Shuffle(startingDeck);

        //Esto es lo nuevo de las manos
        currentPrepHand = 1;
        btnNextHand.gameObject.SetActive(true); // mostrar botón

        cardManager.ClearPanelCard();
        cardManager.DrawFullHand(); // primera mano


        // ---------------PUNTOS DE SPAWN DE ENEMIGOS-----------------
        spawnEnemies.DesactivarLuces();
        spawnEnemies.GenerarPuntosSpawn(numberWave, enemiesToKillInCurrentWave);

    }

    //----------------------------------------------------------
    //                   INICIO FASE DE ACCION
    //----------------------------------------------------------
    public void StartRun()
    {
        cardManager.ClearLastCard();
        endWave = false;
        FindAnyObjectByType<AudioManager>().PlayBackgroundSound(2);
        spawnStarted = false; 
        inPrepPhase = false;
        ChangePhase(GamePhase.Action);
        runButton.SetActive(false);
        //btnNextHand.transform.parent.gameObject.SetActive(false); //ya no hace falta (revisar)
        panelTimeScale.SetActive(true);
        StartCoroutine("PanelGo");

        EliminarCanvasEnemies();

        cardManager.ClearPanelCard();
        cardManager.drawPile = new List<CardData>();

        drawPileImage.SetActive(true);
        cardManager.ResetCardPlayedFlagForAction();
        soulsBar.SetActive(false);
        cardManager.drawPile.Clear();
        //Crear el mazo de cartas de tipo ACCI�N
        foreach (CardData card in startingDeck)
        {
            if (card.cardType != CardType.Trap && card.cardType != CardType.DeckEffect && card.cardType != CardType.Summon)
            {
                cardManager.drawPile.Add(card);
            }
        }

        cardManager.discardPile.Clear();
        DeactivateDiscardPileImage();
        textNumberOfCardsDeck.text = cardManager.drawPile.Count.ToString();

        cardManager.Shuffle(cardManager.drawPile);
        cardManager.DrawFullHand();

        cardManager.ResetMulliganForActionPhase();
        if (cardManager.drawPile.Count > 0)
        {
            bool firstTime = !Prefs.GetBool(PREF_MULLIGAN_SEEN, false);
            if (firstTime)
            {
                mulliganPanel.SetActive(true);
                cardManager.BeginMulligan();
                Prefs.SetBool(Prefs.Keys.MulliganSeenAction, true);
            }
            else
            {
                BeginSpawningEnemies();
                SetupMulliganButton();
            }
        }
        else
        {
            if (mulliganButton) mulliganButton.gameObject.SetActive(false);
            BeginSpawningEnemies();
        }
    }

    private void SetupMulliganButton()
    {
        if (!mulliganButton) return;

        // activo pero solo si aún no jugó carta en esta fase
        bool canUse = !cardManager.cardPlayedThisActionPhase;
        mulliganButton.gameObject.SetActive(true);
        mulliganButton.interactable = canUse;
    }

    public void OpenMulliganFromButton()
    {
        if (cardManager.cardPlayedThisActionPhase) { mulliganButton.interactable = false; return; }

        mulliganPanel.SetActive(true);
        cardManager.BeginMulligan();

        mulliganButton.gameObject.SetActive(false);
    }

    public void OnCardPlayedInAction()
    {
        if (CurrentPhase != GamePhase.Action) return;
        if (mulliganButton) mulliganButton.interactable = false;
    }

    private void BeginSpawningEnemies()
    {
        if (spawnStarted) return;
        spawnStarted = true;
        var spawner = GameObject.Find("SpawnManager").GetComponent<SpawnEnemies>();
        spawner.StartCoroutine(spawner.GenerarEnemigos());
    }

    public void OnMulliganFinished()
    {
        mulliganPanel.SetActive(false);
        mulliganButton.gameObject.SetActive(true);
        mulliganButton.interactable = false;
        messageText.text = "";
        BeginSpawningEnemies();
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

    public void DeactivateDiscardPileImage()
    {
        discardPileImage.SetActive(false);
    }
    public void UpdateTextNumberOfCardsDiscard()
    {
        textNumberOfCardsDiscardPile.text = cardManager.discardPile.Count.ToString();
    }
    public void SkipCountDown()
    {
        handleNextHand.SetActive(false);  //ocultar next hand
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
        playerController.soulsBuyPerShop = 0;
        soulsBar.SetActive(false);
        ChangePhase(GamePhase.Merchant);
        FindAnyObjectByType<AudioManager>().PlayBackgroundSound(0);
        panelMerchant.SetActive(true);
        cameraMovementInstance.EnableCameraControl(false);
        var shopList = new List<MerchantItem>();

        // 1 cartas aleatorias de tipo Trap o Buff
        var all = Resources.LoadAll<CardData>("Cards");
        var pool = new List<CardData>();
        //Elimino las cartas de bufo y habilidad que ya tenga el player para no ofrecerlas
        var ownedSpellNames = new HashSet<string>();
        if (startingDeck != null)
        {
            foreach (var c in startingDeck)
            {
                if (c == null) continue;
                if (c.cardType == CardType.Buff || c.cardType == CardType.Hability)
                    ownedSpellNames.Add(c.cardName);
            }
        }

        foreach (var c in all)
        {
            if ((c.cardType == CardType.Trap || c.cardType == CardType.Buff) && !ownedSpellNames.Contains(c.cardName))
                pool.Add(c);
        }   
        for (int i = 0; i < 2; i++)
        {
            var pick = pool[UnityEngine.Random.Range(0, pool.Count)];
            var cardItem = ScriptableObject.CreateInstance<CardItem>();
            cardItem.itemName = pick.cardName;
            cardItem.cost = pick.goldCost;
            cardItem.cardData = pick;
            shopList.Add(cardItem);
        }

        // 2 cartas “deck effect” (deck cards)
        if (numberWave >= 10)                               // <-- ADD
        {
            pool.Clear();
            foreach (var cards in all)
                if (cards.cardType == CardType.DeckEffect)
                    pool.Add(cards);

            for (int i = 0; i < 2 && pool.Count > 0; i++)
            {
                var pick = pool[UnityEngine.Random.Range(0, pool.Count)];
                var cardItem = ScriptableObject.CreateInstance<CardItem>();
                cardItem.itemName = pick.cardName;
                cardItem.cost = pick.goldCost;
                cardItem.cardData = pick;
                shopList.Add(cardItem);
                pool.Remove(pick);
            }
        }

        // 1 llave
        shopList.Add(keyItemAsset);
        // 1 poción
        shopList.Add(potionItemAsset);

        shopList.Add(soulsItemAsset);

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

    public void NextPreparationHand()
    {
        if (currentPrepHand == totalPrepHands)
        {
            //skipButton.gameObject.SetActive(false);
            handleNextHand.SetActive(false);
            return;
        }
        currentPrepHand++;
        cardManager.DiscardHand();
        cardManager.DrawFullHand();

        UpdatePrepHandUI();

        if(currentPrepHand == totalPrepHands)
        {
            //skipButton.gameObject.SetActive(false);
            handleNextHand.SetActive(false);
        }
        /*if (currentPrepHand > totalPrepHands)
        {
            StartRun();
        }*/
    }
    private void UpdatePrepHandUI()
    {
        prepHandCounterText.text = $"{currentPrepHand}/{totalPrepHands}";
        //if (currentPrepHand == totalPrepHands) nextHandText.text = "Run";
        //else nextHandText.text = "Next Hand";
    }

    public void OpenRoom()
    {
        closedRooms--;
        Debug.Log($"salas cerradas: {closedRooms}");
    }

    public IEnumerator KeyVFX() 
    {        
        yield return new WaitForSecondsRealtime(2f);
        panelKeyInfo.SetActive(false);
    }

}
