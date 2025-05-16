using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private int enemiesDied = 0;
    public int enemiesToKill;
    private int numberWave = 1;

    public TextMeshProUGUI textNumberWave;
    public GameObject panelEndWave;
    public CardManager cardManager;


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

    public void EnemieKaputt()
    {
        enemiesDied++;
        Debug.Log("enemigo " + enemiesDied + " muerto");
        if (enemiesDied >= enemiesToKill)
        {
            EndWave();
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
        List<CardData> selectedCards = FindAnyObjectByType<CardSummaryUI>().selectedCards;
        cardManager.PreparationPhase(selectedCards);
    }
}
