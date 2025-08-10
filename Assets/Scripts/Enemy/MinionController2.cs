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
    //[SerializeField] string trapLayerName = "TrapLayer";

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

        // Limpiar objetivos muertos
        if (currentTarget != null && currentTarget.gameObject == null)
        {
            if (targetAssignments.ContainsKey(currentTarget))
            {
                targetAssignments.Remove(currentTarget);
            }
            currentTarget = null;
        }
    }

    /* ------------------------ Búsqueda ------------------------ */
    void FindTarget()
    {
        //Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, enemyLayer);
        //if (hit != null && hit.CompareTag("Enemy"))
        //    currentTarget = hit.transform;

        // ============= BÚSQUEDA DE ENEMIGOS ÚNICOS PARA ATACAR =============
        // Buscar todos los enemigos en rango
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);

        Transform bestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D enemy in enemies)
        {
            if (!enemy.CompareTag("Enemy")) continue;

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
            if (currentTarget != null && targetAssignments.ContainsKey(currentTarget))
            {
                targetAssignments.Remove(currentTarget);
            }

            // Asignar nuevo objetivo
            currentTarget = bestTarget;
            targetAssignments[currentTarget] = this;
        }
    }

    /* ------------------------ Estados ------------------------ */
    void HandleIdle()
    {
        anim.SetTrigger("idle");
        if (currentTarget != null)
        {
            currentState = State.Chase;
            currentPath.Clear(); // Limpiar path anterior
        }
    }

    void HandleChase()
    {
        if (currentTarget == null)
        {
            currentState = State.Return;
            currentPath.Clear();
            return;
        }

        // Verificar si estamos a distancia de ataque (1 tile)
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        if (distanceToTarget <= attackDistance)
        {
            currentState = State.Attack;
            currentPath.Clear();
            return;
        }

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
            currentState = State.Return;
            currentPath.Clear();
            return;
        }

        // Si el enemigo se aleja demasiado, volver a perseguir
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        if (distanceToTarget > attackDistance + 0.5f)
        {
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
            currentState = State.Chase;
            currentPath.Clear();
            return;
        }

        if (Vector2.Distance(transform.position, originPoint) < 0.1f)
        {
            currentState = State.Idle;
            currentPath.Clear();
            return;
        }

        anim.SetTrigger("run");

        // Calcular path hacia el origen
        if (currentPath.Count == 0)
        {
            currentPath = FindPathBFS(transform.position, originPoint);
            currentPathIndex = 0;
        }

        // Seguir el path
        FollowPath();
    }

    /* -------------------- Pathfinding con BFS -------------------- */
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

                if (IsCellBlocked(next))
                    continue;

                frontier.Enqueue(next);
                cameFrom[next] = current2;
            }
        }

        // Reconstruir path
        List<Vector3> path = new List<Vector3>();
        if (!cameFrom.ContainsKey(goal))
            return path; // No se encontró camino

        Vector3Int current = goal;
        while (current != start)
        {
            path.Add(obstacleTilemap.GetCellCenterWorld(current));
            current = cameFrom[current];
        }

        path.Reverse();
        return path;
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

        // Verificar si hay collider de trampa
        //Collider2D trapCollider = Physics2D.OverlapCircle(worldPos, 0.1f, LayerMask.GetMask(trapLayerName));
        //if (trapCollider != null)
        //    return true;

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

    /* -------------------- Movimiento en cuadrícula -------------------- */

    bool IsTileReserved(Vector3 tilePos)
    {
        foreach (var minion in MinionController2.allMinions)
        {
            if (minion == this) continue;
            if (Vector3.Distance(minion.transform.position, tilePos) < cellSize * 0.1f)
                return true;
            // Aquí puedes también comprobar si el minion tiene planeado moverse a tilePos
        }
        return false;
    }
    void TryMove(Vector2 dir)
    {
        if (isMoving || dir == Vector2.zero) return;

        Vector3 targetWorldPos = SnapToGrid(transform.position) + (Vector3)(dir * cellSize);
        if (IsTileReserved(targetWorldPos)) return; // ¡Ya reservado!
        Vector3Int cell = obstacleTilemap.WorldToCell(targetWorldPos);

        // Verificar que la celda no esté bloqueada antes de moverse
        if (!IsCellBlocked(cell))
            StartCoroutine(MoveToCell(targetWorldPos));
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
        anim.ResetTrigger("run");
        anim.SetTrigger("attack");

        yield return new WaitForSeconds(0.35f);

        if (currentTarget != null && currentState == State.Attack)
        {
            var enemyController = currentTarget.GetComponent<EnemyController>();
            if (enemyController != null)
            {
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
            //if(isFacingRight)
            //{
            //    transform.localScale = Vector3.one;
            //}
            //else
            //{
            //    transform.localScale = new Vector3(-1,1,1);
            //}
            
        }
    }

    // ================== ASIGNACIÓN DE ENEMIGOS ==================




}