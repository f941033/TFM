using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnEnemies : MonoBehaviour
{
    public GameObject enemy;
    public Transform[] spawnWaypoints;
    private List<Transform> spawnListPoints = new List<Transform>();
    private int enemiesToSpawn, enemiesCounter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AddWayPoints(spawnWaypoints,null);
        //StartCoroutine("GenerarEnemigos");
        
        enemiesCounter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator GenerarEnemigos()
    {
        enemiesToSpawn = FindFirstObjectByType<GameManager>().enemiesToKill;

        while (enemiesCounter < enemiesToSpawn)
        {
            yield return new WaitForSeconds(3f);

            if (spawnListPoints.Count == 0)
                continue;

            int randomWP = Random.Range(0, spawnListPoints.Count);
            Transform chosenPoint = spawnListPoints[randomWP];

            Instantiate(enemy, chosenPoint.position, Quaternion.identity);
            enemiesCounter++;
            Debug.Log("enemiesToSpawn: " + enemiesToSpawn + ", enemiesCounter: " + enemiesCounter);
        }

        
    }
    public void AddWayPoints(Transform[] posiciones, Transform[] removePoints)
    {
        RemoveWayPoints(removePoints);
        foreach (Transform point in posiciones)
            if (!spawnListPoints.Contains(point))
                spawnListPoints.Add(point);
    }

    void RemoveWayPoints(Transform[] removePoints)
    {
        if (!removePoints.IsUnityNull())
        foreach (Transform point in removePoints)
            spawnListPoints.Remove(point);
    }
}
