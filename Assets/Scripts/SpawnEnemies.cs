using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnEnemies : MonoBehaviour
{
    public GameObject enemy;
    public Transform[] spawnWaypoints;
    public ArrayList spawnListPoints = new ArrayList();

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
            yield return new WaitForSeconds(3f);

            foreach (Transform point in spawnListPoints)
            {
                Instantiate(enemy, point.position, Quaternion.identity);
            }
        }

        
    }
    public void AddWayPoints(Transform[] posiciones, Transform[] removePoints)
    {
        RemoveWayPoints(removePoints);
        foreach (Transform point in posiciones)
        {
            spawnListPoints.Add(point);
        }
    }

    void RemoveWayPoints(Transform[] removePoints)
    {
        if (!removePoints.IsUnityNull())
        foreach (Transform point in removePoints)
        {
            spawnListPoints.Remove(point);
        }
    }
}
