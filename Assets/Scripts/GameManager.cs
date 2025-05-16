using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private int enemiesDied = 0;
    public int enemiesToKill;
    private int numberWave = 1;
    private List<CardData> selectedCards = new List<CardData>();

    public TextMeshProUGUI textNumberWave;
    public GameObject panelEndWave;
    public CardManager cardManager;
    public CardSummaryUI cardSummaryUI;
    public PlayerController playerController;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        textNumberWave.text = "Ronda: " + numberWave.ToString();
        enemiesToKill = 5;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnemyKaputt()
    {
        enemiesDied++;
        Debug.Log("enemigo " + enemiesDied + " muerto");
        if (enemiesDied == enemiesToKill)
        {
            enemiesDied = 0;
            Invoke("EndWave",1f);
        }
    }

    void EndWave()
    {
        panelEndWave.SetActive(true);

    }


    public void PlayAnotherRun()
    {
        Debug.Log("vamos a por otra ronda");
        panelEndWave.SetActive(false);
        numberWave++;
        textNumberWave.text = "Ronda: " + numberWave;

        playerController.BaseHealth = cardSummaryUI.baseHealth;
        cardManager.PreparationPhase(selectedCards);
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

    //NO SE USA
    public void SetSelectedCards(List<CardData> selectedCards)
    {
        this.selectedCards = selectedCards;
    }
}
