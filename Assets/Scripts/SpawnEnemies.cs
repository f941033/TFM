using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnEnemies : MonoBehaviour
{
    public GameObject enemy;
    public Transform[] spawnWaypoints;
    private List<Transform> spawnListPoints = new List<Transform>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AddWayPoints(spawnWaypoints,null);
        StartCoroutine("GenerarEnemigos");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator GenerarEnemigos()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);

            if (spawnListPoints.Count == 0)
                continue;

            int randomWP = Random.Range(0, spawnListPoints.Count);
            Transform chosenPoint = spawnListPoints[randomWP];

            Instantiate(enemy, chosenPoint.position, Quaternion.identity);
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
