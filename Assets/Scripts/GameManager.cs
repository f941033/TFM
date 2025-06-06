using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    private int enemiesDied = 0;
    public int initialEnemiesToKill = 3;
    public int enemiesToKillInCurrentWave;
    public int numberWave = 1;
    private List<CardData> selectedCards = new List<CardData>();
    private CardData[] allCards;

    public TextMeshProUGUI textNumberWave;
    public GameObject panelEndWave;
    public CardManager cardManager;
    public CardSummaryUI cardSummaryUI;
    public PlayerController playerController;
    public CardSelectionManager selectionManager;
    public Tilemap tilemap;

    public GameObject prefabCard;
    public Transform panelCard;
    [SerializeField] private TextMeshProUGUI messageText;
    private Coroutine HideMessageCO;

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
            Invoke("EndWave", 1f);
        }
    }

    void EndWave()
    {
        Time.timeScale = 1f;
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

        //-----------ELIMINAR FOSOS DE LA RONDA ANTERIOR--------------

        //GameObject[] fosos = GameObject.FindGameObjectsWithTag("Foso");
        //foreach (var item in fosos)
        //{
        //    Vector3Int cellPos = tilemap.WorldToCell(item.transform.position);
        //    tilemap.SetColor(cellPos, Color.white);
        //    Destroy(item);
        //}


        messageText.gameObject.SetActive(false);
        ClearPanelCard();
        panelEndWave.SetActive(false);
        numberWave++;
        textNumberWave.text = "Ronda: " + numberWave;
        enemiesToKillInCurrentWave = Mathf.CeilToInt(initialEnemiesToKill * Mathf.Pow(numberWave, 0.8f));


        /*------------------------------------------------
         *           A�ADIR CARTA NUEVA AL DECK
         *           
        esto no funciona por la obtenci�n del cardData
        CardData selected = selectionManager.GetSelectedCardData();
        {
            selectedCards.Add(selected);
            Debug.Log("carta a�adida: " + selected.cardName);
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
            if (item.cardName == selectedName)
                selectedCards.Add(item);
        }

        cardManager.PreparationPhase(selectedCards);
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

}
