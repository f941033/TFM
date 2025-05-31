using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private int enemiesDied = 0;
    public int initialEnemiesToKill = 3;
    public int enemiesToKillInCurrentWave;
    private int numberWave = 1;
    private List<CardData> selectedCards = new List<CardData>();
    private CardData[] allCards;

    public TextMeshProUGUI textNumberWave;
    public GameObject panelEndWave;
    public CardManager cardManager;
    public CardSummaryUI cardSummaryUI;
    public PlayerController playerController;
    public CardSelectionManager selectionManager;

    public GameObject prefabCard;
    public Transform panelCard;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
            Invoke("EndWave",1f);
        }
    }

    void EndWave()
    {
        panelEndWave.SetActive(true);
        GenerateRewardCard();
    }

    void GenerateRewardCard()
    {
        allCards = Resources.LoadAll<CardData>("Cards");        

        for (int i = 1; i <= 3; i++)
        {
            var cardData = Instantiate(prefabCard, panelCard);
            CardUI cardUI = cardData.GetComponentInChildren<CardUI>();
            int index = Random.Range(0, allCards.Length);
            cardUI.setCardUI(allCards[index]);
        }
    }

    public void PlayAnotherRun()
    {
        Debug.Log("vamos a por otra ronda");
        
        ClearPanelCard();
        panelEndWave.SetActive(false);
        numberWave++;
        textNumberWave.text = "Ronda: " + numberWave;
        enemiesToKillInCurrentWave = Mathf.CeilToInt(initialEnemiesToKill * Mathf.Pow(numberWave, 0.8f));


        /*------------------------------------------------
         *           AÑADIR CARTA NUEVA AL DECK
         *           
        esto no funciona por la obtención del cardDat
        CardData selected = selectionManager.GetSelectedCardData();
        {
            selectedCards.Add(selected);
            Debug.Log("carta añadida: " + selected.cardName);
        }
        else
        {
            Debug.Log("carta nueva sin datos");
        }
        */


        //esto es una chapuza pero funciona:

        string selectedName = selectionManager.GetSelectedCardName();
        allCards = Resources.LoadAll<CardData>("Cards");

        foreach (var item in allCards)
        {
            if(item.cardName == selectedName)
                selectedCards.Add(item);
        }


        cardManager.PreparationPhase(selectedCards);

        //Time.timeScale = 0f;

        //cardSummaryUI.ResetRun();
        //Lo comentado intentando que las oleadas pasen bien
        //cardSummaryUI.ReadyButton();

        /*
         * LO QUE HACE READYBUTTON
        playerController.BaseHealth = baseHealth;
        cardManager.PreparationPhase(selectedCards);

        foreach (Transform child in listContainer) 
            Destroy(child.gameObject);
        selectedCards.Clear();

        canvasDeck.SetActive(false);
         * */
    }


    public void SetSelectedCards(List<CardData> selectedCards)
    {
        this.selectedCards = selectedCards;
    }

    void ClearPanelCard()
    {
        foreach (Transform card in panelCard)
        {
            Destroy(card.gameObject);
        }
    }

}
