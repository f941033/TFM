using UnityEngine;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    [Header("Configuraci�n")]
    public float moveSpeed = 1f;
    public LayerMask obstacleLayer;
    public Vector2 tileSize = new Vector2(1f, 1f);
    Transform player; // Asigna el transform del jugador

    private Vector2 currentDirection;
    private bool isMoving = false;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Transform>();
        StartCoroutine(MoveRoutine());
    }

    IEnumerator MoveRoutine()
    {
        while (true)
        {
            // Calcular direcci�n �ptima hacia el jugador
            Vector2 optimalDirection = CalculateOptimalDirection();

            // Intentar mover en direcci�n �ptima
            if (TryMove(optimalDirection))
            {
                currentDirection = optimalDirection;
            }
            else
            {
                // Buscar direcci�n alternativa
                currentDirection = GetBestAlternativeDirection();
            }

            yield return StartCoroutine(MoveToNextTile(currentDirection));
        }
    }

    Vector2 CalculateOptimalDirection()
    {
        Vector2 playerPosition = player.position;
        Vector2 currentPosition = transform.position;
        Vector2 direction = Vector2.zero;

        // Calcular diferencia en ambos ejes
        float xDiff = playerPosition.x - currentPosition.x;
        float yDiff = playerPosition.y - currentPosition.y;

        // Priorizar el eje con mayor diferencia
        if (Mathf.Abs(xDiff) > Mathf.Abs(yDiff))
        {
            direction.x = xDiff > 0 ? 1 : -1;
        }
        else
        {
            direction.y = yDiff > 0 ? 1 : -1;
        }

        return direction.normalized;
    }

    bool TryMove(Vector2 direction)
    {
        Vector2 nextPosition = (Vector2)transform.position + direction * tileSize;
        return !Physics2D.OverlapBox(nextPosition, tileSize * 0.8f, 0f, obstacleLayer);
    }

    Vector2 GetBestAlternativeDirection()
    {
        Vector2[] directions = {
            Vector2.up, Vector2.down,
            Vector2.left, Vector2.right
        };

        Vector2 bestDirection = currentDirection;
        float minDistance = Mathf.Infinity;
        Vector2 playerPos = player.position;

        foreach (Vector2 dir in directions)
        {
            if (dir == -currentDirection) continue; // Evitar retroceder

            if (TryMove(dir))
            {
                Vector2 potentialPosition = (Vector2)transform.position + dir * tileSize;
                float distance = Vector2.Distance(potentialPosition, playerPos);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestDirection = dir;
                }
            }
        }

        // Si todas las direcciones est�n bloqueadas, forzar movimiento
        return bestDirection != Vector2.zero ? bestDirection : -currentDirection;
    }

    IEnumerator MoveToNextTile(Vector2 direction)
    {
        isMoving = true;
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
        isMoving = false;
    }


    /* -------------- DESTRUCCI�N DE MUROS ---------------
     *                    (NO FUNCIONA)
     * 

    [Header("Destrucci�n de muros")]
    public float attackRange = 0.5f;
    public int attackDamage = 1;
    public float attackCooldown = 2f;
    private float lastAttackTime;
    private bool isAttacking = false;

    [Header("Configuraci�n")]
    public float moveSpeed = 1f;
    public LayerMask obstacleLayer;
    public Vector2 tileSize = new Vector2(1f, 1f);
    Transform player; // Asigna el transform del jugador

    private Vector2 currentDirection;
    private bool isMoving = false;


    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Transform>();
        StartCoroutine(MoveRoutine());
    }


    void Update()
    {
        if (!isMoving && !isAttacking)
        {
            StartCoroutine(MovementLogic());
        }
    }

    IEnumerator MovementLogic()
    {
        isAttacking = false;
        Vector2 optimalDirection = CalculateOptimalDirection();

        if (TryMove(optimalDirection))
        {
            yield return StartCoroutine(MoveToNextTile(optimalDirection));
        }
        else
        {
            // Verificar si el obst�culo es un muro del jugador
            if (IsPlayerWallBlocking(optimalDirection) && Time.time > lastAttackTime + attackCooldown)
            {
                yield return StartCoroutine(AttackWall(optimalDirection));
            }
            else
            {
                Vector2 newDirection = GetBestAlternativeDirection();
                yield return StartCoroutine(MoveToNextTile(newDirection));
            }
        }
    }


    bool IsPlayerWallBlocking(Vector2 direction)
    {
        Vector2 checkPos = (Vector2)transform.position + direction * tileSize;
        Collider2D hit = Physics2D.OverlapBox(checkPos, tileSize * 0.8f, 0f, obstacleLayer);
        return hit != null && hit.CompareTag("PlayerWall");
    }

    IEnumerator AttackWall(Vector2 direction)
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        Vector2 attackPosition = (Vector2)transform.position + direction * tileSize;
        Collider2D wall = Physics2D.OverlapBox(attackPosition, tileSize * 0.8f, 0f, obstacleLayer);

        if (wall != null && wall.CompareTag("PlayerWall"))
        {
            wall.GetComponent<DestructibleWall>().TakeDamage(attackDamage);
            yield return new WaitForSeconds(0.5f); // Tiempo de ataque
        }

        isAttacking = false;
    }

    IEnumerator MoveRoutine()
    {
        while (true)
        {
            while (true)
            {
                // Calcular direcci�n �ptima hacia el jugador
                Vector2 optimalDirection = CalculateOptimalDirection();

                // Si la direcci�n es cero, espera un frame y vuelve a intentar
                if (optimalDirection == Vector2.zero)
                {
                    yield return null;
                    continue;
                }

                // Intentar mover en direcci�n �ptima
                if (TryMove(optimalDirection))
                {
                    currentDirection = optimalDirection;
                }
                else
                {
                    // Buscar direcci�n alternativa
                    currentDirection = GetBestAlternativeDirection();
                }

                yield return StartCoroutine(MoveToNextTile(currentDirection));
            }
        }
    }

    Vector2 CalculateOptimalDirection()
    {
        Vector2 playerPosition = player.position;
        Vector2 currentPosition = transform.position;
        Vector2 direction = Vector2.zero;

        float xDiff = playerPosition.x - currentPosition.x;
        float yDiff = playerPosition.y - currentPosition.y;

        // Solo asigna direcci�n si hay diferencia real
        if (Mathf.Abs(xDiff) > 0.01f || Mathf.Abs(yDiff) > 0.01f)
        {
            if (Mathf.Abs(xDiff) > Mathf.Abs(yDiff))
                direction.x = xDiff > 0 ? 1 : -1;
            else
                direction.y = yDiff > 0 ? 1 : -1;
        }

        return direction;
    }

    Vector2 GetBestAlternativeDirection()
    {
        Vector2[] directions = {
            Vector2.up, Vector2.down,
            Vector2.left, Vector2.right
        };

        Vector2 bestDirection = currentDirection;
        float minDistance = Mathf.Infinity;
        Vector2 playerPos = player.position;

        foreach (Vector2 dir in directions)
        {
            if (dir == -currentDirection) continue; // Evitar retroceder

            if (TryMove(dir))
            {
                Vector2 potentialPosition = (Vector2)transform.position + dir * tileSize;
                float distance = Vector2.Distance(potentialPosition, playerPos);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestDirection = dir;
                }
            }
        }

        // Si todas las direcciones est�n bloqueadas, forzar movimiento
        return bestDirection != Vector2.zero ? bestDirection : -currentDirection;
    }

    IEnumerator MoveToNextTile(Vector2 direction)
    {
        isMoving = true;
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
        isMoving = false;
    }


    bool TryAttackAdjacentWalls()
    {
        Vector2[] directions = {
            Vector2.up, Vector2.down,
            Vector2.left, Vector2.right
        };

        foreach (Vector2 dir in directions)
        {
            Vector2 checkPos = (Vector2)transform.position + dir * tileSize;

            // Detectar muros del jugador usando tag
            Collider2D hit = Physics2D.OverlapBox(
                checkPos,
                tileSize * 0.8f,
                0f,
                obstacleLayer
            );

            if (hit != null && hit.CompareTag("PlayerWall"))
            {
                AttackWall(hit.gameObject);
                return true;
            }
        }
        return false;
    }

    void AttackWall(GameObject wall)
    {
        // Aseg�rate de que el muro tenga un script DestructibleWall
        DestructibleWall wallScript = wall.GetComponent<DestructibleWall>();
        if (wallScript != null)
        {
            wallScript.TakeDamage(attackDamage);
            lastAttackTime = Time.time;
        }
    }

    bool TryMove(Vector2 direction)
    {
        Vector2 nextPosition = (Vector2)transform.position + direction * tileSize;
        Collider2D obstacle = Physics2D.OverlapBox(nextPosition, tileSize * 0.8f, 0f, obstacleLayer);
        return obstacle == null;
    }
    */
}
