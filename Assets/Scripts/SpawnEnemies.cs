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

    public void GenerarPuntosSpawn()
    {
        // Empezamos con un solo punto de spawn

        int randomWP = Random.Range(0, spawnListPoints.Count);
        chosenPoints.Clear();
        chosenPoints.Add(spawnListPoints[randomWP]);

        ActivarLuces();
    }
    private void ActivarLuces()
    {
        foreach (GameObject go in chosenPoints)
        {
            go.transform.GetChild(0).gameObject.SetActive(true);
        }

    }

    public void DesactivarLuces()
    {
        foreach (GameObject go in chosenPoints)
        {
            go.transform.GetChild(0).gameObject.SetActive(false);
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
