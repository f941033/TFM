using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour
{
    [Header("Configuracion")]
    public float moveSpeed = 1f;
    public LayerMask obstacleLayer;
    public Vector2 tileSize = new Vector2(0.5f, 0.5f);
    Transform player;
    Transform currentMoveTarget;
    private bool movingToTrap = false;

    private Vector2 currentDirection;
    private int damage;

    [Header("Knockback")]
    public float knockbackDuration = 0.3f;
    public int knockbackTiles = 3;
    private bool isKnockbackActive = false;

    [Header("Trampas")]
    [SerializeField] private LayerMask trapLayer;
    [SerializeField] private float trapDetectionDistance = 1.2f;

    [Header("Comportamiento de Emergencia")]
    [SerializeField] public bool emergencyMode = false;
    [SerializeField] private Transform currentWallTarget = null;
    [SerializeField] private float wallDetectionRange = 50f;
    [SerializeField] private int wallApproachRadius = 4;
    [SerializeField] private Vector3 wallTargetPosition;

    [Header("LOD System")]
    public EnemyLOD currentLOD = EnemyLOD.High;
    public float highLODDistance = 10f;
    public float mediumLODDistance = 25f;

    // =========== MOVIMIENTO ORTOGONAL Y BFS ====================
    public string playerTag = "Player";
    GameObject playerObj;
    private bool isMoving = false;
    public List<Vector3> path = new List<Vector3>();
    public int pathIndex = 0;
    private bool pathCalculated = false;
    private bool waitingForPath = false;

    private readonly Vector2Int[] directions = {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    void Start()
    {
        playerObj = GameObject.FindWithTag(playerTag);
        if (playerObj == null)
        {
            enabled = false;
            return;
        }
        player = playerObj.transform;
        transform.position = SnapToGrid(transform.position);

        // REGISTRAR en PathfindingManager
        if (PathfindingManager.Instance != null)
            PathfindingManager.Instance.RegisterEnemy(this);

        CheckInitialPathToPlayer();

        // Inicio con delay aleatorio para distribuir cálculos
        float randomOffset = Random.Range(0f, 0.5f);
        StartCoroutine(DelayedStart(randomOffset));
    }

    void OnDestroy()
    {
        // DESREGISTRAR cuando se destruya
        if (PathfindingManager.Instance != null)
            PathfindingManager.Instance.UnregisterEnemy(this);
    }

    IEnumerator DelayedStart(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(MovementLoop());
    }

    void CheckInitialPathToPlayer()
    {
        // Usar PathfindingManager para verificación inicial
        if (PathfindingManager.Instance != null)
        {
            List<Vector3> initialPath = PathfindingManager.Instance.FindPathBFS(
                SnapToGrid(transform.position),
                SnapToGrid(player.position)
            );

            if (initialPath == null || initialPath.Count == 0)
            {
                Debug.Log($"[{gameObject.name}] No hay camino inicial al player. Activando modo de emergencia.");
                ActivateEmergencyMode();
            }
        }
    }

    void UpdateLOD()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= highLODDistance)
            currentLOD = EnemyLOD.High;
        else if (distanceToPlayer <= mediumLODDistance)
            currentLOD = EnemyLOD.Medium;
        else
            currentLOD = EnemyLOD.Low;
    }

    IEnumerator MovementLoop()
    {
        while (true)
        {
            UpdateLOD();

            float waitTime = currentLOD switch
            {
                EnemyLOD.High => 0.15f,
                EnemyLOD.Medium => 0.5f,
                EnemyLOD.Low => 1f,
                _ => 0.15f
            };

            if (!isMoving && !waitingForPath)
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
            yield return new WaitForSeconds(waitTime);
        }
    }

    IEnumerator HandleNormalMovement()
    {
        // Solicitar path al PathfindingManager
        waitingForPath = true;
        pathCalculated = false;

        if (PathfindingManager.Instance != null)
        {
            PathfindingManager.Instance.RequestPath(this,
                SnapToGrid(transform.position),
                SnapToGrid(player.position),
                false);
        }

        // Esperar hasta recibir el path
        yield return new WaitUntil(() => pathCalculated);
        waitingForPath = false;

        if (path != null && path.Count > 0)
        {
            pathIndex = 1;
            while (pathIndex < path.Count)
            {
                Vector3 nextTile = path[pathIndex];
                yield return StartCoroutine(MoveToTile(nextTile));
                pathIndex++;
            }
        }
    }

    IEnumerator HandleEmergencyMovement()
    {
        Transform nearestWall = null;
        if (WallCache.Instance != null)
        {
            nearestWall = WallCache.Instance.GetNearestWall(transform.position, wallDetectionRange);
        }
        else
        {
            nearestWall = FindNearestReachableWall();
        }

        if (nearestWall != null)
        {
            Vector3 approachPosition = FindBestApproachPosition(nearestWall);

            if (approachPosition != Vector3.zero)
            {
                Debug.Log($"[{gameObject.name}] Moviéndose hacia muro en {nearestWall.position}. Posición objetivo: {approachPosition}");

                // Solicitar path hacia el muro usando PathfindingManager
                waitingForPath = true;
                pathCalculated = false;

                if (PathfindingManager.Instance != null)
                {
                    PathfindingManager.Instance.RequestPath(this,
                        SnapToGrid(transform.position),
                        SnapToGrid(approachPosition),
                        true);
                }

                yield return new WaitUntil(() => pathCalculated);
                waitingForPath = false;

                if (path != null && path.Count > 0)
                {
                    currentWallTarget = nearestWall;
                    wallTargetPosition = approachPosition;

                    pathIndex = 1;
                    while (pathIndex < path.Count && currentWallTarget != null)
                    {
                        Vector3 nextTile = path[pathIndex];
                        yield return StartCoroutine(MoveToTile(nextTile));
                        pathIndex++;
                    }

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

    public void OnPathCalculated(List<Vector3> calculatedPath, bool isEmergency)
    {
        if (calculatedPath != null && calculatedPath.Count > 0)
        {
            path = calculatedPath;
        }
        else
        {
            path = null;
            if (!isEmergency && !emergencyMode)
            {
                Debug.Log($"[{gameObject.name}] No hay camino al player. Activando modo de emergencia.");
                ActivateEmergencyMode();
            }
        }
        pathCalculated = true;
    }

    Transform FindNearestReachableWall()
    {
        GameObject[] playerWalls = GameObject.FindGameObjectsWithTag("PlayerWall");

        if (playerWalls.Length == 0)
        {
            Debug.Log($"[{gameObject.name}] No se encontraron muros con tag PlayerWall en la escena");
            return null;
        }

        Transform bestWall = null;
        float bestDistance = float.MaxValue;
        Vector3 currentPos = SnapToGrid(transform.position);

        foreach (GameObject wall in playerWalls)
        {
            float distance = Vector3.Distance(currentPos, wall.transform.position);

            if (distance <= wallDetectionRange)
            {
                Vector3 approachPos = FindBestApproachPosition(wall.transform);

                if (approachPos != Vector3.zero)
                {
                    if (PathfindingManager.Instance != null)
                    {
                        List<Vector3> testPath = PathfindingManager.Instance.FindPathBFS(currentPos, SnapToGrid(approachPos));

                        if (testPath != null && testPath.Count > 0)
                        {
                            if (distance < bestDistance)
                            {
                                bestDistance = distance;
                                bestWall = wall.transform;
                            }
                        }
                    }
                }
            }
        }

        return bestWall;
    }

    Vector3 FindBestApproachPosition(Transform wall)
    {
        if (wall == null) return Vector3.zero;

        Vector3 wallPos = SnapToGrid(wall.position);
        Vector2Int wallGrid = WorldToGrid(wallPos);

        for (int radius = 1; radius <= wallApproachRadius; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius) continue;

                    Vector2Int checkGrid = wallGrid + new Vector2Int(x, y);

                    if (!IsObstacle(checkGrid))
                    {
                        Vector3 checkWorld = GridToWorld(checkGrid);

                        if (PathfindingManager.Instance != null)
                        {
                            List<Vector3> testPath = PathfindingManager.Instance.FindPathBFS(
                                SnapToGrid(transform.position), checkWorld);

                            if (testPath != null && testPath.Count > 0)
                            {
                                return checkWorld;
                            }
                        }
                    }
                }
            }
        }

        return Vector3.zero;
    }

    void NotifyWallReached(Transform wall)
    {
        EnemyController enemyController = GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.NotifyWallFound(wall);
        }
    }

    public void OnWallDestroyed()
    {
        currentWallTarget = null;

        if (PathfindingManager.Instance != null)
        {
            List<Vector3> pathToPlayer = PathfindingManager.Instance.FindPathBFS(
                SnapToGrid(transform.position),
                SnapToGrid(player.position)
            );

            if (pathToPlayer != null && pathToPlayer.Count > 0)
            {
                Debug.Log($"[{gameObject.name}] Camino al player restaurado. Desactivando modo de emergencia.");
                DeactivateEmergencyMode();
            }
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
        if (c == null) return false;
        if (c.CompareTag("PlayerWall") || c.gameObject.layer == LayerMask.NameToLayer("Obstacle")) return true;
        return false;
    }

    // ==================== MÉTODOS PÚBLICOS ====================
    public void ClearAndRepath()
    {
        StopAllCoroutines();
        isMoving = false;
        waitingForPath = false;
        pathCalculated = false;
        path.Clear();
        pathIndex = 0;

        transform.position = SnapToGrid(transform.position);

        if (PathfindingManager.Instance != null)
        {
            List<Vector3> pathToPlayer = PathfindingManager.Instance.FindPathBFS(
                SnapToGrid(transform.position),
                SnapToGrid(player.position)
            );

            if (pathToPlayer == null || pathToPlayer.Count == 0)
            {
                if (!emergencyMode)
                {
                    ActivateEmergencyMode();
                }
            }
            else if (emergencyMode)
            {
                DeactivateEmergencyMode();
            }
        }

        StartCoroutine(MovementLoop());
    }

    public void RestartCoroutineMove()
    {
        StopAllCoroutines();
        path.Clear();
        pathIndex = 0;
        waitingForPath = false;
        pathCalculated = false;

        transform.position = SnapToGrid(transform.position);
        StartCoroutine(DelayedRestart());
    }

    IEnumerator DelayedRestart()
    {
        yield return new WaitForSeconds(0.1f);

        if (PathfindingManager.Instance != null)
        {
            List<Vector3> pathToPlayer = PathfindingManager.Instance.FindPathBFS(
                SnapToGrid(transform.position),
                SnapToGrid(player.position)
            );

            if (pathToPlayer == null || pathToPlayer.Count == 0)
            {
                ActivateEmergencyMode();
            }
        }

        StartCoroutine(MovementLoop());
    }

    public void StopCoroutineMove()
    {
        StopAllCoroutines();
        waitingForPath = false;
        pathCalculated = false;
    }

    public void ReduceSpeed(float speedPercent, int seconds)
    {
        if (speedPercent == 0f) return;
        if(name.Contains("Heroe")) return;
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

    // ================ ONDA EXPANSIVA DE LA BOMBA ================
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
        StopCoroutine("MoveToTile");

        Vector2 knockbackDirection = -currentDirection;
        Vector2 startPos = transform.position;

        // NUEVA LÓGICA: Calcular hasta dónde puede llegar sin atravesar obstáculos
        Vector2 finalPos = CalculateKnockbackWithCollision(startPos, knockbackDirection);

        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            transform.position = Vector2.Lerp(startPos, finalPos, elapsed / knockbackDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Asegurar posición final alineada al grid
        transform.position = SnapToGrid(finalPos);

        isKnockbackActive = false;
        GetComponent<EnemyController>().ReceiveDamage(damage);
        ClearAndRepath();
    }

    // NUEVO MÉTODO: Calcular knockback con detección de colisiones
    Vector2 CalculateKnockbackWithCollision(Vector2 startPos, Vector2 knockbackDirection)
    {
        Vector2 currentPos = startPos;
        Vector2 normalizedDirection = knockbackDirection.normalized;

        // Verificar tile por tile en la dirección del knockback
        for (int i = 1; i <= knockbackTiles; i++)
        {
            Vector2 nextPos = startPos + (normalizedDirection * tileSize.magnitude * i);
            Vector2 snappedNextPos = SnapToGrid(nextPos);

            // Si la siguiente posición es un obstáculo, detenerse en la actual
            if (IsObstacleAtWorldPosition(snappedNextPos))
            {
                Debug.Log($"[{gameObject.name}] Knockback detenido en tile {i - 1} por obstáculo en {snappedNextPos}");
                break;
            }

            // Si la posición es válida, actualizar la posición actual
            currentPos = snappedNextPos;
        }

        // Si no se pudo mover ni un tile, intentar posiciones adyacentes
        if (Vector2.Distance(currentPos, startPos) < 0.1f)
        {
            currentPos = FindAdjacentValidPosition(SnapToGrid(startPos));
        }

        return currentPos;
    }

    // NUEVO MÉTODO: Verificar si hay obstáculo en una posición del mundo
    bool IsObstacleAtWorldPosition(Vector2 worldPos)
    {
        Vector2Int gridPos = WorldToGrid(worldPos);
        return IsObstacle(gridPos);
    }

    // NUEVO MÉTODO: Buscar una posición adyacente válida si no se puede hacer knockback
    Vector2 FindAdjacentValidPosition(Vector2 currentPos)
    {
        // Verificar las 4 direcciones cardinales adyacentes
        Vector2[] adjacentDirections = {
        Vector2.up * tileSize.y,
        Vector2.down * tileSize.y,
        Vector2.left * tileSize.x,
        Vector2.right * tileSize.x
    };

        foreach (Vector2 direction in adjacentDirections)
        {
            Vector2 adjacentPos = currentPos + direction;
            Vector2 snappedPos = SnapToGrid(adjacentPos);

            if (!IsObstacleAtWorldPosition(snappedPos))
            {
                Debug.Log($"[{gameObject.name}] Knockback mínimo a posición adyacente: {snappedPos}");
                return snappedPos;
            }
        }

        // Si no hay posiciones adyacentes válidas, quedarse donde está
        Debug.Log($"[{gameObject.name}] No se puede aplicar knockback - rodeado de obstáculos");
        return currentPos;
    }

    // MÉTODO MEJORADO: Versión más precisa del SnapToGrid para knockback
    Vector2 SnapToGrid(Vector2 pos)
    {
        float x = Mathf.Round(pos.x / tileSize.x) * tileSize.x;
        float y = Mathf.Round(pos.y / tileSize.y) * tileSize.y;
        return new Vector2(x, y);
    }

    // NUEVO MÉTODO: Buscar posición válida para knockback
    //Vector2 FindValidKnockbackPosition(Vector2 startPos, Vector2 knockbackDirection)
    //{
    //    Vector2 originalTarget = startPos + (knockbackDirection * tileSize * knockbackTiles);
    //    Vector2 snappedTarget = SnapToGrid(originalTarget);

    //    // Si la posición objetivo es válida, usarla
    //    if (IsValidPosition(snappedTarget))
    //    {
    //        return snappedTarget;
    //    }

    //    // Si no es válida, buscar la posición más cercana válida
    //    return FindNearestValidPosition(snappedTarget, startPos);
    //}

    // NUEVO MÉTODO: Verificar si una posición es válida (no es obstáculo)
    bool IsValidPosition(Vector2 worldPos)
    {
        Vector2Int gridPos = WorldToGrid(worldPos);
        return !IsObstacle(gridPos);
    }

    // NUEVO MÉTODO: Buscar la posición válida más cercana
    //Vector2 FindNearestValidPosition(Vector2 targetPos, Vector2 originalPos)
    //{
    //    // Lista de posiciones candidatas, empezando por la más cercana
    //    List<Vector2> candidates = new List<Vector2>();

    //    Vector2 snappedOriginal = SnapToGrid(originalPos);

    //    // Buscar en un radio creciente alrededor de la posición objetivo
    //    for (int radius = 0; radius <= knockbackTiles + 2; radius++)
    //    {
    //        // Agregar posiciones en el radio actual
    //        for (int x = -radius; x <= radius; x++)
    //        {
    //            for (int y = -radius; y <= radius; y++)
    //            {
    //                // Solo verificar posiciones en el borde del radio (no el interior ya verificado)
    //                if (radius == 0 || Mathf.Abs(x) == radius || Mathf.Abs(y) == radius)
    //                {
    //                    Vector2 candidate = targetPos + new Vector2(x * tileSize.x, y * tileSize.y);

    //                    // Verificar que no sea la posición original del enemigo
    //                    if (Vector2.Distance(candidate, snappedOriginal) > 0.1f && IsValidPosition(candidate))
    //                    {
    //                        candidates.Add(candidate);
    //                    }
    //                }
    //            }
    //        }

    //        // Si encontramos candidatos en este radio, elegir el más cercano en la dirección del knockback
    //        if (candidates.Count > 0)
    //        {
    //            return GetBestKnockbackCandidate(candidates, originalPos, targetPos);
    //        }
    //    }

    //    // Si no se encuentra ninguna posición válida, mantener la posición original
    //    Debug.LogWarning($"[{gameObject.name}] No se encontró posición válida para knockback. Manteniendo posición original.");
    //    return snappedOriginal;
    //}

    // NUEVO MÉTODO: Elegir el mejor candidato basado en la dirección del knockback
    //Vector2 GetBestKnockbackCandidate(List<Vector2> candidates, Vector2 originalPos, Vector2 targetPos)
    //{
    //    Vector2 knockbackDir = (targetPos - originalPos).normalized;
    //    Vector2 bestCandidate = candidates[0];
    //    float bestScore = float.MinValue;

    //    foreach (Vector2 candidate in candidates)
    //    {
    //        Vector2 candidateDir = (candidate - originalPos).normalized;
    //        float distance = Vector2.Distance(originalPos, candidate);

    //        // Puntuación basada en:
    //        // 1. Qué tan alineado está con la dirección del knockback (más importante)
    //        // 2. Qué tan lejos está del punto original (knockback más fuerte es mejor)
    //        float directionScore = Vector2.Dot(candidateDir, knockbackDir) * 2f;  // Peso x2 para dirección
    //        float distanceScore = distance / (knockbackTiles * tileSize.magnitude); // Normalizado

    //        float totalScore = directionScore + distanceScore;

    //        if (totalScore > bestScore)
    //        {
    //            bestScore = totalScore;
    //            bestCandidate = candidate;
    //        }
    //    }

    //    return bestCandidate;
    //}


    // ===========  OLD ======================
    //IEnumerator KnockbackCoroutine()
    //{
    //    isKnockbackActive = true;
    //    StopCoroutine("MoveToTile");

    //    Vector2 knockbackDirection = -currentDirection;
    //    Vector2 startPos = transform.position;
    //    Vector2 targetPos = startPos + (knockbackDirection * tileSize * knockbackTiles);

    //    float elapsed = 0f;

    //    while (elapsed < knockbackDuration)
    //    {
    //        transform.position = Vector2.Lerp(startPos, targetPos, elapsed / knockbackDuration);
    //        elapsed += Time.deltaTime;
    //        yield return null;
    //    }

    //    transform.position = targetPos;
    //    isKnockbackActive = false;
    //    GetComponent<EnemyController>().ReceiveDamage(damage);
    //    ClearAndRepath();
    //}

    //====================  TRAMPERO  ====================//
    public void SetTrapTarget(Transform trap)
    {
        if (trap != null)
        {
            Vector2 trapPos = trap.position;
            Vector2 currentPos = transform.position;
            Vector2 direction = (currentPos - trapPos).normalized;
            Vector2 safePosition = trapPos + direction * trapDetectionDistance;

            GameObject tempTarget = new GameObject("TrapTarget");
            tempTarget.transform.position = safePosition;
            currentMoveTarget = tempTarget.transform;
            movingToTrap = true;
        }
    }

    public void ClearTrapTarget()
    {
        if (movingToTrap && currentMoveTarget != player)
        {
            if (currentMoveTarget != null && currentMoveTarget.gameObject.name == "TrapTarget")
            {
                Destroy(currentMoveTarget.gameObject);
            }
        }

        currentMoveTarget = player;
        movingToTrap = false;
    }
}

// Enum para el sistema LOD
public enum EnemyLOD
{
    High,    // Cerca del player
    Medium,  // Distancia media
    Low      // Lejos del player
}