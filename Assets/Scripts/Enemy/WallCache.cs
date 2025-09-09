using UnityEngine;
using System.Collections.Generic;

public class WallCache : MonoBehaviour
{
    private static WallCache instance;
    public static WallCache Instance => instance;

    [Header("Cache Settings")]
    public float refreshInterval = 2f; // Actualizar cache cada X segundos

    private List<Transform> cachedWalls = new List<Transform>();
    private Dictionary<Vector3, List<Vector3>> pathCache = new Dictionary<Vector3, List<Vector3>>();
    private float lastRefreshTime = 0f;

    void Awake()
    {
        instance = this;
        RefreshWallCache();
    }

    void Update()
    {
        // Actualizar cache periódicamente
        if (Time.time - lastRefreshTime > refreshInterval)
        {
            RefreshWallCache();
            lastRefreshTime = Time.time;
        }
    }

    public void RefreshWallCache()
    {
        cachedWalls.Clear();
        GameObject[] walls = GameObject.FindGameObjectsWithTag("PlayerWall");

        foreach (var wall in walls)
        {
            if (wall != null)
                cachedWalls.Add(wall.transform);
        }

        // Limpiar cache de paths cuando se actualiza la cache de muros
        pathCache.Clear();

        Debug.Log($"WallCache: Actualizado con {cachedWalls.Count} muros");
    }

    public Transform GetNearestWall(Vector3 position, float maxDistance)
    {
        Transform nearest = null;
        float nearestDist = float.MaxValue;

        // Limpiar muros destruidos
        cachedWalls.RemoveAll(wall => wall == null);

        foreach (var wall in cachedWalls)
        {
            if (wall == null) continue;

            float dist = Vector3.Distance(position, wall.position);
            if (dist < maxDistance && dist < nearestDist)
            {
                nearest = wall;
                nearestDist = dist;
            }
        }

        return nearest;
    }

    public List<Transform> GetWallsInRange(Vector3 position, float maxDistance)
    {
        List<Transform> wallsInRange = new List<Transform>();

        // Limpiar muros destruidos
        cachedWalls.RemoveAll(wall => wall == null);

        foreach (var wall in cachedWalls)
        {
            if (wall == null) continue;

            float dist = Vector3.Distance(position, wall.position);
            if (dist < maxDistance)
            {
                wallsInRange.Add(wall);
            }
        }

        return wallsInRange;
    }

    // Llamar cuando se destruya un muro para actualizar inmediatamente
    public void OnWallDestroyed(Transform destroyedWall)
    {
        cachedWalls.Remove(destroyedWall);
        pathCache.Clear(); // Limpiar cache de paths
        Debug.Log($"WallCache: Muro destruido removido de cache. Quedan {cachedWalls.Count} muros");
    }

    // Llamar cuando se cree un nuevo muro
    public void OnWallCreated(Transform newWall)
    {
        if (!cachedWalls.Contains(newWall))
        {
            cachedWalls.Add(newWall);
            pathCache.Clear(); // Limpiar cache de paths
            Debug.Log($"WallCache: Nuevo muro añadido a cache. Total: {cachedWalls.Count} muros");
        }
    }

    public int GetWallCount()
    {
        // Limpiar nulls antes de contar
        cachedWalls.RemoveAll(wall => wall == null);
        return cachedWalls.Count;
    }
}