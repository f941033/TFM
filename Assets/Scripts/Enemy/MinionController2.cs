using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class MinionController2 : MonoBehaviour
{
    /* --------------- Ajustes visibles en el Inspector --------------- */

    [Header("Coordinación")]
    public static Dictionary<Transform, MinionController2> targetAssignments = new Dictionary<Transform, MinionController2>();

    [Header("Detección")]
    [SerializeField] float detectionRadius = 8f;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] Tilemap obstacleTilemap;
    [SerializeField] string obstacleLayerName = "Obstacle";

    [Header("Combate")]
    [SerializeField] int damage = 10;
    [SerializeField] float timeBetweenHits = 2f;
    [SerializeField] private float health;
    [SerializeField] public float currentHealth;
    public Image healthBarUI;

    [Header("Movimiento en cuadrícula")]
    [SerializeField] float cellSize = 1f;
    [SerializeField] float moveDuration = 0.25f;
    [SerializeField] float attackDistance = 1.5f; // Distancia de ataque (1 tile + margen)

    /* ---------------  Variables internas --------------- */

    public static List<MinionController2> allMinions = new List<MinionController2>();

    Transform currentTarget;
    Vector3 originPoint;
    bool isFacingRight = true;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    bool isMoving = false;
    bool canAttack = true;

    enum State { Idle, Chase, Attack, Return, Dead }
    State currentState = State.Idle;

    // Variables para pathfinding
    private List<Vector3> currentPath = new List<Vector3>();
    private int currentPathIndex = 0;
    private Vector3 lastTargetPosition;

    // Direcciones para pathfinding (4 direcciones ortogonales)
    private Vector3Int[] directions = {
        Vector3Int.right,  // Este
        Vector3Int.left,   // Oeste  
        Vector3Int.up,     // Norte
        Vector3Int.down    // Sur
    };

    void Awake() { allMinions.Add(this); }
    void OnDestroy()
    {
        allMinions.Remove(this);

        if (currentTarget != null && targetAssignments.ContainsKey(currentTarget))
        {
            targetAssignments.Remove(currentTarget);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        if (obstacleTilemap == null)
            obstacleTilemap = GameObject.Find("Paredes")?.GetComponent<Tilemap>();

        originPoint = SnapToGrid(transform.position);
        lastTargetPosition = Vector3.zero;

        currentHealth = health;
    }

    void Update()
    {
        // CORRECCIÓN: Verificar estado del objetivo ANTES de buscar nuevo
        CheckTargetStatus();

        if (currentState != State.Dead && currentTarget == null)
            FindTarget();

        switch (currentState)
        {
            case State.Idle: HandleIdle(); break;
            case State.Chase: HandleChase(); break;
            case State.Attack: HandleAttack(); break;
            case State.Return: HandleReturn(); break;
            case State.Dead: HandleDead(); break;
        }
    }

    // NUEVO: Método para verificar el estado del objetivo actual
    void CheckTargetStatus()
    {
        if (currentTarget == null) return;

        bool shouldClearTarget = false;

        // Verificar si el GameObject fue destruido
        if (currentTarget.gameObject == null)
        {
            shouldClearTarget = true;
        }
        // Verificar si el enemigo está "muerto" (health <= 0 o componente EnemyController inactivo)
        else
        {
            EnemyController enemyController = currentTarget.GetComponent<EnemyController>();
            if (enemyController == null || !currentTarget.gameObject.activeInHierarchy)
            {
                shouldClearTarget = true;
            }
            // Verificar si el enemigo tiene salud <= 0
            else
            {
                // Usando reflexión para acceder a currentHealth (ya que es private)
                var healthField = typeof(EnemyController).GetField("currentHealth",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (healthField != null)
                {
                    float enemyHealth = (float)healthField.GetValue(enemyController);
                    if (enemyHealth <= 0)
                    {
                        shouldClearTarget = true;
                    }
                }
            }
        }

        if (shouldClearTarget)
        {
            Debug.Log($"[{gameObject.name}] Objetivo eliminado. Cambiando a estado Return.");
            ClearCurrentTarget();

            // Forzar cambio de estado a Return si estaba atacando
            if (currentState == State.Attack || currentState == State.Chase)
            {
                currentState = State.Return;
                currentPath.Clear();
            }
        }
    }

    // NUEVO: Método para limpiar el objetivo actual
    void ClearCurrentTarget()
    {
        if (currentTarget != null && targetAssignments.ContainsKey(currentTarget))
        {
            targetAssignments.Remove(currentTarget);
        }
        currentTarget = null;
    }

    /* ------------------------ Búsqueda ------------------------ */
    void FindTarget()
    {
        // ============= BÚSQUEDA DE ENEMIGOS ÚNICOS PARA ATACAR =============
        // Buscar todos los enemigos en rango
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);

        Transform bestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D enemy in enemies)
        {
            if (!enemy.CompareTag("Enemy")) continue;

            // Verificar que el enemigo esté vivo y activo
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController == null || !enemy.gameObject.activeInHierarchy) continue;

            // Verificar si este enemigo YA está siendo atacado por otro minion
            if (targetAssignments.ContainsKey(enemy.transform) &&
                targetAssignments[enemy.transform] != null &&
                targetAssignments[enemy.transform] != this)
            {
                continue; // Saltar este enemigo, ya está ocupado
            }

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestTarget = enemy.transform;
            }
        }

        // Asignar el nuevo objetivo
        if (bestTarget != null)
        {
            // Liberar objetivo anterior si tenía uno
            ClearCurrentTarget();

            // Asignar nuevo objetivo
            currentTarget = bestTarget;
            targetAssignments[currentTarget] = this;
            Debug.Log($"[{gameObject.name}] Nuevo objetivo asignado: {currentTarget.name}");
        }
    }

    /* ------------------------ Estados ------------------------ */
    void HandleIdle()
    {
        // CORRECCIÓN: Limpiar triggers conflictivos y asegurar solo idle
        anim.ResetTrigger("run");
        anim.ResetTrigger("attack");
        anim.SetTrigger("idle");

        if (currentTarget != null)
        {
            Debug.Log($"[{gameObject.name}] Idle -> Chase");
            currentState = State.Chase;
            currentPath.Clear(); // Limpiar path anterior
        }
    }

    void HandleChase()
    {
        if (currentTarget == null)
        {
            Debug.Log($"[{gameObject.name}] Chase -> Return (no target)");
            currentState = State.Return;
            currentPath.Clear();
            return;
        }

        // Verificar si estamos a distancia de ataque (1 tile)
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        if (distanceToTarget <= attackDistance)
        {
            Debug.Log($"[{gameObject.name}] Chase -> Attack (in range)");
            currentState = State.Attack;
            currentPath.Clear();
            return;
        }

        // CORRECCIÓN: Limpiar triggers conflictivos antes de run
        anim.ResetTrigger("idle");
        anim.ResetTrigger("attack");
        anim.SetTrigger("run");

        // Calcular path hacia el enemigo, pero detenerse a 1 tile de distancia
        Vector3 targetPos = GetPositionOneThileAway(currentTarget.position);

        // Recalcular path si el objetivo se movió o no tenemos path
        if (Vector3.Distance(targetPos, lastTargetPosition) > 0.1f || currentPath.Count == 0)
        {
            currentPath = FindPathBFS(transform.position, targetPos);
            currentPathIndex = 0;
            lastTargetPosition = targetPos;
        }

        // Seguir el path
        FollowPath();
    }

    void HandleAttack()
    {
        if (currentTarget == null)
        {
            Debug.Log($"[{gameObject.name}] Attack -> Return (no target)");
            currentState = State.Return;
            currentPath.Clear();
            return;
        }

        // Si el enemigo se aleja demasiado, volver a perseguir
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        if (distanceToTarget > attackDistance + 0.5f)
        {
            Debug.Log($"[{gameObject.name}] Attack -> Chase (target moved away)");
            currentState = State.Chase;
            currentPath.Clear();
            return;
        }

        // Mirar hacia el enemigo
        Vector2 dirToEnemy = (currentTarget.position - transform.position).normalized;
        Flip(dirToEnemy);

        if (canAttack)
            StartCoroutine(PerformAttack());
    }

    void HandleReturn()
    {
        if (currentTarget != null)
        {
            Debug.Log($"[{gameObject.name}] Return -> Chase (new target found)");
            currentState = State.Chase;
            currentPath.Clear();
            return;
        }

        float distanceToOrigin = Vector2.Distance(transform.position, originPoint);

        // CORRECCIÓN: Tolerancia mayor y snap al origen
        if (distanceToOrigin < 0.3f)
        {
            Debug.Log($"[{gameObject.name}] Return -> Idle (reached origin)");
            currentState = State.Idle;
            currentPath.Clear();

            // Snap exacto al origen y forzar idle
            rb.MovePosition(originPoint);
            anim.ResetTrigger("run");
            anim.ResetTrigger("attack");
            anim.SetTrigger("idle");
            return;
        }

        // CORRECCIÓN: Solo activar run si realmente se está moviendo
        if (isMoving)
        {
            anim.ResetTrigger("idle");
            anim.ResetTrigger("attack");
            anim.SetTrigger("run");
        }
        else
        {
            // Si no se está moviendo, forzar idle para evitar conflictos
            anim.ResetTrigger("run");
            anim.ResetTrigger("attack");
            anim.SetTrigger("idle");
        }

        // CORRECCIÓN: Recalcular path más frecuentemente si está bloqueado
        if (currentPath.Count == 0 || Vector3.Distance(transform.position, currentPath[Mathf.Min(currentPathIndex, currentPath.Count - 1)]) > 0.5f)
        {
            Debug.Log($"[{gameObject.name}] Recalculando path hacia origen");
            currentPath = FindPathBFS(transform.position, originPoint);
            currentPathIndex = 0;
        }

        // Seguir el path
        FollowPath();

        // CORRECCIÓN: Si el path está vacío o completado pero aún no llegó, intentar acercarse directamente
        if (currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
        {
            if (!isMoving)
            {
                // Si no hay path pero está cerca, ir directamente
                if (distanceToOrigin < 2f)
                {
                    Debug.Log($"[{gameObject.name}] Intentando movimiento directo hacia origen");
                    Vector2 directDirection = GetCardinalDirection(originPoint - transform.position);
                    TryMove(directDirection);
                }
                else
                {
                    Debug.Log($"[{gameObject.name}] Path completado pero no en origen. Forzando Idle.");
                    currentState = State.Idle;
                    rb.MovePosition(originPoint);
                    anim.ResetTrigger("run");
                    anim.ResetTrigger("attack");
                    anim.SetTrigger("idle");
                }
            }
        }
    }

    /* -------------------- Pathfinding con BFS MEJORADO -------------------- */

    List<Vector3> FindPathBFS(Vector3 startWorld, Vector3 goalWorld)
    {
        if (obstacleTilemap == null) return new List<Vector3>();

        Vector3Int start = obstacleTilemap.WorldToCell(startWorld);
        Vector3Int goal = obstacleTilemap.WorldToCell(goalWorld);

        Queue<Vector3Int> frontier = new Queue<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();

        frontier.Enqueue(start);
        cameFrom[start] = start;

        while (frontier.Count > 0)
        {
            Vector3Int current2 = frontier.Dequeue();

            if (current2 == goal)
                break;

            foreach (Vector3Int direction in directions)
            {
                Vector3Int next = current2 + direction;

                if (cameFrom.ContainsKey(next))
                    continue;

                // CORRECCIÓN: Considerar tanto obstáculos estáticos como minions
                if (IsCellBlockedIncludingMinions(next))
                    continue;

                frontier.Enqueue(next);
                cameFrom[next] = current2;
            }
        }

        // Reconstruir path
        List<Vector3> path = new List<Vector3>();
        if (!cameFrom.ContainsKey(goal))
        {
            // NUEVO: Si no hay camino directo, intentar path más flexible
            return FindFlexiblePath(startWorld, goalWorld);
        }

        Vector3Int current = goal;
        while (current != start)
        {
            path.Add(obstacleTilemap.GetCellCenterWorld(current));
            current = cameFrom[current];
        }

        path.Reverse();
        return path;
    }

    // NUEVO: Método que considera minions como obstáculos temporales
    bool IsCellBlockedIncludingMinions(Vector3Int cellPosition)
    {
        // Verificar obstáculos estáticos primero
        if (IsCellBlocked(cellPosition))
            return true;

        // Verificar si hay otros minions en esta posición
        Vector3 worldPos = obstacleTilemap.GetCellCenterWorld(cellPosition);

        foreach (var minion in MinionController2.allMinions)
        {
            if (minion == this) continue; // No considerarse a sí mismo

            // Si otro minion está en esta celda, considerarla bloqueada
            if (Vector3.Distance(minion.transform.position, worldPos) < cellSize * 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    // NUEVO: Pathfinding más flexible para cuando hay bloqueos temporales
    List<Vector3> FindFlexiblePath(Vector3 startWorld, Vector3 goalWorld)
    {
        if (obstacleTilemap == null) return new List<Vector3>();

        Vector3Int start = obstacleTilemap.WorldToCell(startWorld);
        Vector3Int goal = obstacleTilemap.WorldToCell(goalWorld);

        // Buscar posiciones alternativas cerca del objetivo
        List<Vector3Int> alternativeGoals = GetAlternativeGoals(goal);

        foreach (Vector3Int altGoal in alternativeGoals)
        {
            Queue<Vector3Int> frontier = new Queue<Vector3Int>();
            Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();

            frontier.Enqueue(start);
            cameFrom[start] = start;

            while (frontier.Count > 0)
            {
                Vector3Int current = frontier.Dequeue();

                if (current == altGoal)
                {
                    // Encontró camino a objetivo alternativo
                    List<Vector3> path = new List<Vector3>();
                    Vector3Int step = altGoal;
                    while (step != start)
                    {
                        path.Add(obstacleTilemap.GetCellCenterWorld(step));
                        step = cameFrom[step];
                    }
                    path.Reverse();

                    Debug.Log($"[{gameObject.name}] Encontrado camino alternativo con {path.Count} pasos");
                    return path;
                }

                foreach (Vector3Int direction in directions)
                {
                    Vector3Int next = current + direction;

                    if (cameFrom.ContainsKey(next))
                        continue;

                    // Solo considerar obstáculos estáticos, ignorar minions temporalmente
                    if (IsCellBlocked(next))
                        continue;

                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        Debug.Log($"[{gameObject.name}] No se encontró camino flexible");
        return new List<Vector3>();
    }

    // NUEVO: Generar objetivos alternativos cerca del objetivo original
    List<Vector3Int> GetAlternativeGoals(Vector3Int originalGoal)
    {
        List<Vector3Int> alternatives = new List<Vector3Int>();

        // Agregar el objetivo original primero
        alternatives.Add(originalGoal);

        // Agregar posiciones en radio creciente
        for (int radius = 1; radius <= 3; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    // Solo el borde del radio actual
                    if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius) continue;

                    Vector3Int altPos = originalGoal + new Vector3Int(x, y, 0);

                    // Verificar que no sea un obstáculo estático
                    if (!IsCellBlocked(altPos))
                    {
                        alternatives.Add(altPos);
                    }
                }
            }
        }

        return alternatives;
    }

    bool IsCellBlocked(Vector3Int cellPosition)
    {
        // Verificar si hay tile de obstáculo
        if (obstacleTilemap.HasTile(cellPosition))
            return true;

        Vector3 worldPos = obstacleTilemap.GetCellCenterWorld(cellPosition);

        // Verificar si hay collider de obstáculo normal
        Collider2D obstacleCollider = Physics2D.OverlapCircle(worldPos, 0.1f, LayerMask.GetMask(obstacleLayerName));
        if (obstacleCollider != null)
            return true;

        return false;
    }

    Vector3 GetPositionOneThileAway(Vector3 targetPosition)
    {
        Vector3 myPos = SnapToGrid(transform.position);
        Vector3 targetPos = SnapToGrid(targetPosition);

        Vector3 direction = (myPos - targetPos).normalized;
        Vector2 cardinalDir = GetCardinalDirection(direction);

        return targetPos + (Vector3)(cardinalDir * cellSize);
    }

    void FollowPath()
    {
        if (currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
            return;

        Vector3 targetCell = currentPath[currentPathIndex];
        Vector2 direction = GetCardinalDirection(targetCell - transform.position);

        // Si estamos cerca del waypoint actual, pasar al siguiente
        if (Vector3.Distance(transform.position, targetCell) < 0.1f)
        {
            currentPathIndex++;
            return;
        }

        // Mover hacia el waypoint actual
        TryMove(direction);
        Flip(direction);
    }

    /* -------------------- Movimiento en cuadrícula MEJORADO -------------------- */

    bool IsTileReserved(Vector3 tilePos)
    {
        foreach (var minion in MinionController2.allMinions)
        {
            if (minion == this) continue;
            if (Vector3.Distance(minion.transform.position, tilePos) < cellSize * 0.1f)
                return true;
        }
        return false;
    }

    void TryMove(Vector2 dir)
    {
        if (isMoving || dir == Vector2.zero) return;

        Vector3 targetWorldPos = SnapToGrid(transform.position) + (Vector3)(dir * cellSize);
        Vector3Int cell = obstacleTilemap.WorldToCell(targetWorldPos);

        // Verificar obstáculos estáticos
        if (IsCellBlocked(cell))
            return;

        // CORRECCIÓN: Si hay un minion, esperar un poco e intentar recalcular path
        if (IsTileReserved(targetWorldPos))
        {
            // No moverse ahora, pero en el próximo frame intentar nuevo path
            StartCoroutine(RecalculatePathAfterDelay());
            return;
        }

        StartCoroutine(MoveToCell(targetWorldPos));
    }

    // NUEVO: Recalcular path después de un pequeño delay
    IEnumerator RecalculatePathAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);

        // Solo recalcular si estamos en Return y no hay target
        if (currentState == State.Return && currentTarget == null)
        {
            Debug.Log($"[{gameObject.name}] Recalculando path por bloqueo de minion");
            currentPath.Clear();
            currentPathIndex = 0;
        }
    }

    IEnumerator MoveToCell(Vector3 destino)
    {
        isMoving = true;
        Vector3 origen = transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            rb.MovePosition(Vector3.Lerp(origen, destino, t));
            yield return null;
        }

        rb.MovePosition(destino);
        isMoving = false;
    }

    /* ------------------------ Combate ------------------------ */
    IEnumerator PerformAttack()
    {
        canAttack = false;

        // CORRECCIÓN: Limpiar triggers antes de attack
        anim.ResetTrigger("run");
        anim.ResetTrigger("idle");
        anim.SetTrigger("attack");

        yield return new WaitForSeconds(0.35f);

        if (currentTarget != null && currentState == State.Attack)
        {
            var enemyController = currentTarget.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                Debug.Log($"[{gameObject.name}] Atacando a {currentTarget.name}");
                enemyController.ReceiveDamage(damage);
                enemyController.NotifyAttackedByMinion(this);
            }
        }

        yield return new WaitForSeconds(timeBetweenHits);
        canAttack = true;
    }

    public void ReceiveDamage(float damage)
    {
        currentHealth -= damage;
        healthBarUI.fillAmount = currentHealth / health;
        if (currentHealth <= 0)
        {
            currentState = State.Dead;
        }
    }

    /* ------------------------ Muerte ------------------------ */

    void HandleDead()
    {
        Destroy(gameObject);
    }

    /* ------------------------ Utilidades ------------------------ */
    Vector2 GetCardinalDirection(Vector3 rawDir)
    {
        if (Mathf.Abs(rawDir.x) > Mathf.Abs(rawDir.y))
            return rawDir.x > 0 ? Vector2.right : Vector2.left;
        else
            return rawDir.y > 0 ? Vector2.up : Vector2.down;
    }

    Vector3 SnapToGrid(Vector3 worldPos)
    {
        if (obstacleTilemap == null) return worldPos;

        Vector3Int cell = obstacleTilemap.WorldToCell(worldPos);
        return obstacleTilemap.GetCellCenterWorld(cell);
    }

    void Flip(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) < 0.1f) return;

        bool shouldFaceRight = dir.x > 0;
        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = shouldFaceRight;
            sr.flipX = !isFacingRight;
        }
    }
}