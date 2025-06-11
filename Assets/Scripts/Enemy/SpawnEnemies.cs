using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnEnemies : MonoBehaviour
{
    public GameObject enemy;
    public GameObject[] spawnWaypoints;
    private List<GameObject> spawnListPoints = new List<GameObject>();
    private List<GameObject> chosenPoints = new List<GameObject>();
    private int enemiesToSpawn, enemiesCounter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AddWayPoints(spawnWaypoints,null);
        //StartCoroutine("GenerarEnemigos");
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerarPuntosSpawn(int roundNumber)
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


    public IEnumerator GenerarEnemigos()
    {
        enemiesToSpawn = FindFirstObjectByType<GameManager>().enemiesToKillInCurrentWave;
        enemiesCounter = 0;

        for (int i = 1;i<=enemiesToSpawn;i++)
        {
            yield return new WaitForSeconds(Random.Range(1.5f,3f));

            if (chosenPoints.Count == 0)
                continue;

            int randomWP = Random.Range(0, chosenPoints.Count);
            Transform chosenPoint = chosenPoints[randomWP].transform;

            Instantiate(enemy, chosenPoint.position, Quaternion.identity);
            enemiesCounter++;
            Debug.Log("enemiesToSpawn: " + enemiesToSpawn + ", enemiesCounter: " + enemiesCounter);
        }

        
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
