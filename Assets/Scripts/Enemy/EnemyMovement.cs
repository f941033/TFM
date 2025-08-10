using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour
{
    [Header("Configuracion")]
    public float moveSpeed = 1f;
    public LayerMask obstacleLayer;
    public Vector2 tileSize = new Vector2(0.5f, 0.5f);
    Transform player; // Asigna el transform del jugador
    Transform currentMoveTarget; // TRAMPERO: objetivo actual de movimiento (puede ser player o posición cerca de trampa)
    private bool movingToTrap = false; // TRAMPERO: flag para saber si se está moviendo hacia una trampa

    private Vector2 currentDirection;
    private int damage;

    [Header("Knockback")]
    public float knockbackDuration = 0.2f; // Duración del desplazamiento
    public int knockbackTiles = 5; // Celdas a retroceder
    private bool isKnockbackActive = false;

    [Header("Trampas")]
    [SerializeField] private LayerMask trapLayer; // TRAMPERO: capa de las trampas
    [SerializeField] private float trapDetectionDistance = 1.2f; // TRAMPERO: distancia para detectar trampas en el camino

    [Header("Comportamiento de Emergencia")]
    [SerializeField] public bool emergencyMode = false; // Flag para modo de emergencia (público para HeroeMovement)
    [SerializeField] private Transform currentWallTarget = null; // Muro objetivo actual
    [SerializeField] private float wallDetectionRange = 50f; // Rango para buscar muros
    [SerializeField] private int wallApproachRadius = 4; // Radio para buscar posiciones cerca del muro
    [SerializeField] private Vector3 wallTargetPosition; // Posición objetivo junto al muro

    // =========== MOVIMIENTO ORTOGONAL Y BFS ====================

    public string playerTag = "Player";
    GameObject playerObj;
    private bool isMoving = false;
    public List<Vector3> path = new List<Vector3>();
    public int pathIndex = 0;
    private readonly Vector2Int[] directions = {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    void Start()
    {
        playerObj = GameObject.FindWithTag(playerTag);
        if (playerObj == null)
        {
            Debug.LogError("No se encontró Player con Tag " + playerTag);
            enabled = false;
            return;
        }
        player = playerObj.transform;
        transform.position = SnapToGrid(transform.position);

        // Debug inicial
        Debug.Log($"[{gameObject.name}] Iniciado. TileSize: {tileSize}. Posición inicial: {transform.position}");

        // Verificar si hay camino al player al inicio
        CheckInitialPathToPlayer();

        StartCoroutine(MovementLoop());
    }

    void CheckInitialPathToPlayer()
    {
        List<Vector3> initialPath = FindPathBFS(SnapToGrid(transform.position), SnapToGrid(player.position));
        if (initialPath == null || initialPath.Count == 0)
        {
            Debug.Log($"[{gameObject.name}] No hay camino inicial al player. Activando modo de emergencia.");
            ActivateEmergencyMode();
        }
        else
        {
            Debug.Log($"[{gameObject.name}] Camino inicial encontrado con {initialPath.Count} pasos.");
        }
    }

    IEnumerator MovementLoop()
    {
        while (true)
        {
            if (!isMoving)
            {
                if (emergencyMode)
                {
                    yield return StartCoroutine(HandleEmergencyMovement());
                }
                else
                {
                    yield return StartCoroutine(HandleNormalMovement());
                }
            }
            yield return new WaitForSeconds(0.15f); // balance rendimiento/respuesta
        }
    }

    IEnumerator HandleNormalMovement()
    {
        List<Vector3> newPath = FindPathBFS(SnapToGrid(transform.position), SnapToGrid(player.position));
        if (newPath != null && newPath.Count > 0)
        {
            // El primer nodo del path es la tile actual, el segundo el destino inmediato
            path = newPath;
            pathIndex = 1;
            while (pathIndex < path.Count)
            {
                Vector3 nextTile = path[pathIndex];
                yield return StartCoroutine(MoveToTile(nextTile));
                pathIndex++;
            }
        }
        else
        {
            // No hay camino, activar modo de emergencia
            Debug.Log($"[{gameObject.name}] Camino perdido. Activando modo de emergencia.");
            ActivateEmergencyMode();
        }
    }

    IEnumerator HandleEmergencyMovement()
    {
        // Simplificar: buscar muro más cercano y moverse hacia él
        Transform nearestWall = FindNearestReachableWall();

        if (nearestWall != null)
        {
            Vector3 approachPosition = FindBestApproachPosition(nearestWall);

            if (approachPosition != Vector3.zero)
            {
                Debug.Log($"[{gameObject.name}] Moviéndose hacia muro en {nearestWall.position}. Posición objetivo: {approachPosition}");

                // Intentar moverse hacia la posición de aproximación
                List<Vector3> wallPath = FindPathBFS(SnapToGrid(transform.position), SnapToGrid(approachPosition));

                if (wallPath != null && wallPath.Count > 0)
                {
                    currentWallTarget = nearestWall;
                    wallTargetPosition = approachPosition;

                    path = wallPath;
                    pathIndex = 1;
                    while (pathIndex < path.Count && currentWallTarget != null)
                    {
                        Vector3 nextTile = path[pathIndex];
                        yield return StartCoroutine(MoveToTile(nextTile));
                        pathIndex++;
                    }

                    // Una vez llegado cerca del muro, notificar al EnemyController para que lo ataque
                    if (currentWallTarget != null)
                    {
                        Debug.Log($"[{gameObject.name}] Llegó cerca del muro. Iniciando ataque.");
                        NotifyWallReached(currentWallTarget);
                    }
                }
                else
                {
                    Debug.Log($"[{gameObject.name}] No se pudo calcular path hacia posición de aproximación {approachPosition}");
                    yield return new WaitForSeconds(1f);
                }
            }
            else
            {
                Debug.Log($"[{gameObject.name}] No se encontró posición de aproximación válida para el muro");
                yield return new WaitForSeconds(1f);
            }
        }
        else
        {
            Debug.Log($"[{gameObject.name}] No se encontraron muros alcanzables. Esperando...");
            yield return new WaitForSeconds(2f);
        }
    }

    Transform FindNearestReachableWall()
    {
        // Buscar todos los objetos con tag PlayerWall
        GameObject[] playerWalls = GameObject.FindGameObjectsWithTag("PlayerWall");

        if (playerWalls.Length == 0)
        {
            Debug.Log($"[{gameObject.name}] No se encontraron muros con tag PlayerWall en la escena");
            return null;
        }

        Debug.Log($"[{gameObject.name}] Evaluando {playerWalls.Length} muros...");

        Transform bestWall = null;
        float bestDistance = float.MaxValue;
        Vector3 bestApproachPos = Vector3.zero;

        Vector3 currentPos = SnapToGrid(transform.position);

        foreach (GameObject wall in playerWalls)
        {
            float distance = Vector3.Distance(currentPos, wall.transform.position);

            if (distance <= wallDetectionRange)
            {
                // Buscar posición de aproximación para este muro
                Vector3 approachPos = FindBestApproachPosition(wall.transform);

                if (approachPos != Vector3.zero)
                {
                    // Verificar que se puede llegar a la posición de aproximación
                    List<Vector3> testPath = FindPathBFS(currentPos, SnapToGrid(approachPos));

                    if (testPath != null && testPath.Count > 0)
                    {
                        if (distance < bestDistance)
                        {
                            bestDistance = distance;
                            bestWall = wall.transform;
                            bestApproachPos = approachPos;
                        }
                    }
                }
            }
        }

        if (bestWall != null)
        {
            Debug.Log($"[{gameObject.name}] Mejor muro encontrado a distancia {bestDistance}. Posición aproximación: {bestApproachPos}");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] No se encontraron muros alcanzables de {playerWalls.Length} muros evaluados");
        }

        return bestWall;
    }

    Vector3 FindBestApproachPosition(Transform wall)
    {
        if (wall == null) return Vector3.zero;

        Vector3 wallPos = SnapToGrid(wall.position);
        Vector2Int wallGrid = WorldToGrid(wallPos);

        // Buscar en un radio expandido alrededor del muro
        for (int radius = 1; radius <= wallApproachRadius; radius++)
        {
            // Buscar en todas las direcciones del radio actual
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    // Solo verificar posiciones en el borde del radio (no el interior)
                    if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius) continue;

                    Vector2Int checkGrid = wallGrid + new Vector2Int(x, y);

                    // Verificar que la posición no sea un obstáculo
                    if (!IsObstacle(checkGrid))
                    {
                        Vector3 checkWorld = GridToWorld(checkGrid);

                        // Verificar que se puede llegar a esta posición
                        List<Vector3> testPath = FindPathBFS(SnapToGrid(transform.position), checkWorld);
                        if (testPath != null && testPath.Count > 0)
                        {
                            Debug.Log($"[{gameObject.name}] Posición de aproximación encontrada para muro: {checkWorld} (radio {radius})");
                            return checkWorld;
                        }
                    }
                }
            }
        }

        Debug.Log($"[{gameObject.name}] No se encontró posición de aproximación válida para muro en {wallPos}");
        return Vector3.zero;
    }

    void NotifyWallReached(Transform wall)
    {
        // Notificar al EnemyController que debe atacar este muro
        EnemyController enemyController = GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.NotifyWallFound(wall);
        }
    }

    public void OnWallDestroyed()
    {
        // Llamado cuando se destruye un muro
        currentWallTarget = null;

        // Verificar si ahora hay camino al player
        List<Vector3> pathToPlayer = FindPathBFS(SnapToGrid(transform.position), SnapToGrid(player.position));
        if (pathToPlayer != null && pathToPlayer.Count > 0)
        {
            Debug.Log($"[{gameObject.name}] Camino al player restaurado. Desactivando modo de emergencia.");
            DeactivateEmergencyMode();
        }
        else
        {
            Debug.Log($"[{gameObject.name}] Camino aún bloqueado. Continuando en modo de emergencia.");
        }
    }

    void ActivateEmergencyMode()
    {
        emergencyMode = true;
        currentWallTarget = null;
    }

    public void DeactivateEmergencyMode()
    {
        emergencyMode = false;
        currentWallTarget = null;
    }

    IEnumerator MoveToTile(Vector3 destino)
    {
        isMoving = true;

        Vector3 origen = transform.position;
        // Calcular y almacenar la dirección de movimiento
        Vector2 moveDir = (destino - origen).normalized;
        currentDirection = moveDir;

        float distance = Vector3.Distance(origen, destino);
        float duration = distance / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(origen, destino, t);
            yield return null;
        }

        transform.position = destino;
        isMoving = false;
    }


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

    // --- Tile utilities ---
    public Vector3 SnapToGrid(Vector3 pos)
    {
        float x = Mathf.Round(pos.x / tileSize.x) * tileSize.x;
        float y = Mathf.Round(pos.y / tileSize.y) * tileSize.y;
        return new Vector3(x, y, pos.z);
    }

    Vector2Int WorldToGrid(Vector3 pos)
    {
        return new Vector2Int(Mathf.RoundToInt(pos.x / tileSize.x), Mathf.RoundToInt(pos.y / tileSize.y));
    }
    Vector3 GridToWorld(Vector2Int g)
    {
        return new Vector3(g.x * tileSize.x, g.y * tileSize.y, transform.position.z);
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

    // ==================== MÉTODOS PÚBLICOS (PRESERVADOS) ====================

    public void ClearAndRepath()
    {
        StopAllCoroutines();
        isMoving = false;
        path.Clear();
        pathIndex = 0;

        // Centrar en el grid
        transform.position = SnapToGrid(transform.position);

        // Verificar si necesitamos modo de emergencia después del repath
        List<Vector3> pathToPlayer = FindPathBFS(SnapToGrid(transform.position), SnapToGrid(player.position));
        if (pathToPlayer == null || pathToPlayer.Count == 0)
        {
            if (!emergencyMode)
            {
                Debug.Log($"[{gameObject.name}] Repath: No hay camino, activando modo emergencia.");
                ActivateEmergencyMode();
            }
        }
        else if (emergencyMode)
        {
            Debug.Log($"[{gameObject.name}] Repath: Camino encontrado, desactivando modo emergencia.");
            DeactivateEmergencyMode();
        }

        // Reiniciar MovementLoop
        StartCoroutine(MovementLoop());
    }

    public void RestartCoroutineMove()
    {
        StopAllCoroutines();
        path.Clear();
        pathIndex = 0;

        // Asegurar posición alineada al grid
        transform.position = SnapToGrid(transform.position);

        // Pequeña pausa para asegurar que todo esté limpio
        StartCoroutine(DelayedRestart());
    }

    IEnumerator DelayedRestart()
    {
        yield return new WaitForSeconds(0.1f); // Pequeña pausa

        // Verificar estado de emergency antes de reiniciar
        List<Vector3> pathToPlayer = FindPathBFS(SnapToGrid(transform.position), SnapToGrid(player.position));
        if (pathToPlayer == null || pathToPlayer.Count == 0)
        {
            ActivateEmergencyMode();
        }

        StartCoroutine(MovementLoop());
        Debug.Log("MovementLoop reiniciado");
    }

    public void StopCoroutineMove()
    {
        StopAllCoroutines();
    }

    IEnumerator MoveToNextTile(Vector2 direction)
    {
        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos + direction * tileSize;
        float elapsed = 0f;

        while (elapsed < 1f / moveSpeed)
        {
            transform.position = Vector2.Lerp(startPos, targetPos, elapsed * moveSpeed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
    }

    public void ReduceSpeed(float speedPercent, int seconds)
    {
        if (speedPercent == 0f) return;
        StartCoroutine(ReduceSpeedCoroutine(speedPercent, seconds));

    }

    IEnumerator ReduceSpeedCoroutine(float speedPercent, int seconds)
    {
        yield return new WaitForSeconds(1.5f);
        float speedTemp = moveSpeed;
        moveSpeed *= speedPercent;
        yield return new WaitForSeconds(seconds);
        moveSpeed = speedTemp;
    }

    //------------ EFECTO ONDA EXPANSIVA DE LA BOMBA -------------------
    public void ApplyKnockback(int damage)
    {
        if (!isKnockbackActive)
        {
            this.damage = damage;
            StartCoroutine(KnockbackCoroutine());
        }
    }

    IEnumerator KnockbackCoroutine()
    {
        isKnockbackActive = true;
        StopCoroutine("MoveToTile"); // Detiene el movimiento normal

        Vector2 knockbackDirection = -currentDirection; // Dirección contraria al movimiento
        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos + (knockbackDirection * tileSize * knockbackTiles);

        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            transform.position = Vector2.Lerp(startPos, targetPos, elapsed / knockbackDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        isKnockbackActive = false;
        GetComponent<EnemyController>().ReceiveDamage(damage);
        ClearAndRepath();
    }

    //====================  TRAMPERO  ====================//

    // Método para establecer que debe moverse hacia una trampa
    public void SetTrapTarget(Transform trap)
    {
        if (trap != null)
        {
            // Calcular posición a una distancia segura de la trampa
            Vector2 trapPos = trap.position;
            Vector2 currentPos = transform.position;
            Vector2 direction = (currentPos - trapPos).normalized;
            Vector2 safePosition = trapPos + direction * trapDetectionDistance;

            // Crear un GameObject temporal para marcar la posición objetivo
            GameObject tempTarget = new GameObject("TrapTarget");
            tempTarget.transform.position = safePosition;
            currentMoveTarget = tempTarget.transform;
            movingToTrap = true;
        }
    }

    // Método para limpiar objetivo de trampa y volver al player
    public void ClearTrapTarget()
    {
        if (movingToTrap && currentMoveTarget != player)
        {
            // Destruir el GameObject temporal si existe
            if (currentMoveTarget != null && currentMoveTarget.gameObject.name == "TrapTarget")
            {
                Destroy(currentMoveTarget.gameObject);
            }
        }

        currentMoveTarget = player;
        movingToTrap = false;
    }
}