using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathfindingManager : MonoBehaviour
{
    private static PathfindingManager instance;
    public static PathfindingManager Instance => instance;

    [Header("Performance")]
    public int maxPathCalculationsPerFrame = 5;
    public float pathUpdateInterval = 0.5f;

    [Header("Grid Configuration")]
    public Vector2 tileSize = new Vector2(0.5f, 0.5f);
    public LayerMask obstacleLayer;

    private Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
    private List<EnemyMovement> activeEnemies = new List<EnemyMovement>();

    // Direcciones para BFS
    private readonly Vector2Int[] directions = {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    void Awake()
    {
        instance = this;
        StartCoroutine(ProcessPathRequests());
    }

    public void RegisterEnemy(EnemyMovement enemy)
    {
        if (!activeEnemies.Contains(enemy))
            activeEnemies.Add(enemy);
    }

    public void UnregisterEnemy(EnemyMovement enemy)
    {
        activeEnemies.Remove(enemy);
    }

    IEnumerator ProcessPathRequests()
    {
        while (true)
        {
            int processed = 0;

            while (pathRequestQueue.Count > 0 && processed < maxPathCalculationsPerFrame)
            {
                PathRequest request = pathRequestQueue.Dequeue();
                if (request.enemy != null)
                {
                    List<Vector3> path = FindPathBFS(request.start, request.goal);
                    request.enemy.OnPathCalculated(path, request.isEmergency);
                }
                processed++;
            }

            yield return null; // Distribuir cálculos entre frames
        }
    }

    public void RequestPath(EnemyMovement enemy, Vector3 start, Vector3 goal, bool isEmergency = false)
    {
        pathRequestQueue.Enqueue(new PathRequest(enemy, start, goal, isEmergency));
    }

    // MÉTODO BFS CENTRALIZADO
    public List<Vector3> FindPathBFS(Vector3 startWorld, Vector3 goalWorld)
    {
        Vector2Int start = WorldToGrid(startWorld);
        Vector2Int goal = WorldToGrid(goalWorld);

        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> prev = new Dictionary<Vector2Int, Vector2Int>();
        frontier.Enqueue(start);
        prev[start] = start;

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();
            if (current == goal) break;

            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;
                if (prev.ContainsKey(next)) continue;
                if (IsObstacle(next)) continue;

                frontier.Enqueue(next);
                prev[next] = current;
            }
        }

        if (!prev.ContainsKey(goal)) return null; // no path

        // Reconstruir path desde goal a start
        List<Vector3> result = new List<Vector3>();
        for (Vector2Int step = goal; ;)
        {
            result.Add(GridToWorld(step));
            if (step == start) break;
            step = prev[step];
        }
        result.Reverse();
        return result;
    }

    // MÉTODOS DE UTILIDAD
    Vector2Int WorldToGrid(Vector3 pos)
    {
        return new Vector2Int(Mathf.RoundToInt(pos.x / tileSize.x), Mathf.RoundToInt(pos.y / tileSize.y));
    }

    Vector3 GridToWorld(Vector2Int g)
    {
        return new Vector3(g.x * tileSize.x, g.y * tileSize.y, 0f);
    }

    bool IsObstacle(Vector2Int gridPos)
    {
        Vector3 tileCenter = GridToWorld(gridPos);
        Collider2D c = Physics2D.OverlapBox(tileCenter, tileSize * 0.8f, 0, obstacleLayer);
        // Incluye paredes y muros del jugador (tag PlayerWall)
        if (c == null) return false;
        if (c.CompareTag("PlayerWall") || c.gameObject.layer == LayerMask.NameToLayer("Obstacle")) return true;
        return false;
    }
}

[System.Serializable]
public struct PathRequest
{
    public EnemyMovement enemy;
    public Vector3 start;
    public Vector3 goal;
    public bool isEmergency;

    public PathRequest(EnemyMovement enemy, Vector3 start, Vector3 goal, bool isEmergency = false)
    {
        this.enemy = enemy;
        this.start = start;
        this.goal = goal;
        this.isEmergency = isEmergency;
    }
}