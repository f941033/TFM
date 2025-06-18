using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnEnemies : MonoBehaviour
{
    public GameObject canvasSpawnPointPrefab;
    public GameObject canvasHeroeInfoPrefab;
    public GameObject enemyPrefab, heroePrefab;
    public GameObject[] spawnWaypoints;
    private List<GameObject> spawnListPoints = new List<GameObject>();
    private List<GameObject> chosenPoints = new List<GameObject>();
    private Dictionary<GameObject, int> enemigosPorPuerta;
    private Transform heroePoint;
    //private int enemiesToSpawn, enemiesCounter;
    bool isSpecialRound = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AddWayPoints(spawnWaypoints, null);
        //StartCoroutine("GenerarEnemigos");


    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GenerarPuntosSpawn(int roundNumber, int enemiesToKillInCurrentWave)
    {
        chosenPoints.Clear();

        //número de puntos de spawn
        int numberOfSpawnPoints = roundNumber >= 20 ? 4 : Mathf.Clamp((roundNumber - 1) / 5 + 1, 1, 4);

        // Crear una copia temporal de la lista original
        List<GameObject> tempList = new List<GameObject>(spawnListPoints);

        // Algoritmo Fisher-Yates para mezclar la lista
        for (int i = 0; i < tempList.Count; i++)
        {
            int randomIndex = Random.Range(i, tempList.Count);
            GameObject temp = tempList[randomIndex];
            tempList[randomIndex] = tempList[i];
            tempList[i] = temp;
        }

        // Tomar los primeros 'numberOfSpawnPoints' elementos
        for (int i = 0; i < numberOfSpawnPoints && i < tempList.Count; i++)
        {
            chosenPoints.Add(tempList[i]);
        }


        ActivarLuces();
        CalcularNumeroEnemigos(roundNumber, enemiesToKillInCurrentWave);
    }
    private void ActivarLuces()
    {
        foreach (GameObject spawnPoint in chosenPoints)
        {

            for (var i = 0; i < spawnPoint.transform.childCount; i++)
            {
                spawnPoint.transform.GetChild(i).GetComponent<Animator>().SetBool("enabled", true);
                spawnPoint.transform.GetChild(i).GetChild(0).gameObject.SetActive(true);
            }

        }

    }

    public void DesactivarLuces()
    {

        foreach (GameObject spawnPoint in chosenPoints)
        {

            for (var i = 0; i < spawnPoint.transform.childCount; i++)
            {
                spawnPoint.transform.GetChild(i).GetComponent<Animator>().SetBool("enabled", false);
                spawnPoint.transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
            }

        }        
    }


    void CalcularNumeroEnemigos(int roundNumber, int enemiesToKillInCurrentWave)
    {
        //enemiesToSpawn = enemiesToKillInCurrentWave;
        isSpecialRound = (roundNumber % 5 == 0);

        enemigosPorPuerta = new Dictionary<GameObject, int>();

        for (int i = 1; i <= enemiesToKillInCurrentWave; i++)
        {
            if (chosenPoints.Count == 0)
                continue;

            int randomWP = Random.Range(0, chosenPoints.Count);
            Transform chosenPoint = chosenPoints[randomWP].transform;
            enemigosPorPuerta[chosenPoint.gameObject] = enemigosPorPuerta.ContainsKey(chosenPoint.gameObject) ?
                                            enemigosPorPuerta[chosenPoint.gameObject] + 1 : 1;

        }

        if (isSpecialRound)
        {
            FindFirstObjectByType<GameManager>().enemiesToKillInCurrentWave++;
            int randomWP = Random.Range(0, chosenPoints.Count);
            heroePoint = chosenPoints[randomWP].transform;
            
        }

        ActivarPanelInfo();
    }

    void ActivarPanelInfo()
    {
        foreach (var puerta in chosenPoints)
        {
            GameObject canvas = Instantiate(canvasSpawnPointPrefab, puerta.transform);
            TextMeshProUGUI textoPanel = canvas.GetComponentInChildren<TextMeshProUGUI>();
            textoPanel.text = "x" + enemigosPorPuerta[puerta].ToString();

            if(isSpecialRound && puerta.transform == heroePoint)
            {
                canvas.transform.Find("PanelInfoHeroe").gameObject.SetActive(true);
            }
            else
            {
                canvas.transform.GetChild(0).Find("PanelInfoHeroe").gameObject.SetActive(false);
            }
        }

    }
    public IEnumerator GenerarEnemigos()
    { 
        //enemiesCounter = 0;

        // ---------------- INSTANCIAR EL HÉROE CADA 5 RONDAS -------------------

        if (isSpecialRound)
        {
            //FindFirstObjectByType<GameManager>().enemiesToKillInCurrentWave++;
            //int randomWP = Random.Range(0, chosenPoints.Count);
            //Transform chosenPoint = chosenPoints[randomWP].transform;
            //Instantiate(heroePrefab, chosenPoint.position, Quaternion.identity);
            Instantiate(heroePrefab, heroePoint.position, Quaternion.identity);
            //enemiesCounter++;
        }

        foreach (var puerta in enemigosPorPuerta.Keys)
        {
            for (int i = 0; i < enemigosPorPuerta[puerta]; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.75f, 1.15f));

                Vector2 offset = new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f));
                Vector2 spawnPosition = new Vector2(puerta.transform.position.x, puerta.transform.position.y) + offset;
                Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                //enemiesCounter++;
            }
        }



        //for (int i = 1; i <= enemiesToSpawn; i++)
        //{
        //    yield return new WaitForSeconds(Random.Range(0.75f,1.15f));

        //    if (chosenPoints.Count == 0)
        //        continue;

        //    int randomWP = Random.Range(0, chosenPoints.Count);
        //    Transform chosenPoint = chosenPoints[randomWP].transform;

        //    Vector2 offset = new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f));
        //    Vector2 spawnPosition = new Vector2(chosenPoint.position.x, chosenPoint.position.y) + offset;

        //    Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        //    enemiesCounter++;
        //}        

    }



    public void AddWayPoints(GameObject[] posiciones, GameObject[] removePoints)
    {
        RemoveWayPoints(removePoints);
        foreach (GameObject point in posiciones)
            if (!spawnListPoints.Contains(point))
                spawnListPoints.Add(point);
    }

    void RemoveWayPoints(GameObject[] removePoints)
    {
        if (!removePoints.IsUnityNull())
        foreach (GameObject point in removePoints)
            spawnListPoints.Remove(point);
    }
}
