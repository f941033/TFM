using UnityEngine;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    [Header("Configuraci�n")]
    public float moveSpeed = 1f;
    public LayerMask obstacleLayer;
    public Vector2 tileSize = new Vector2(1f, 1f);
    Transform player; // Asigna el transform del jugador
    Transform currentMoveTarget; // TRAMPERO: objetivo actual de movimiento (puede ser player o posición cerca de trampa)
    private bool movingToTrap = false; // TRAMPERO: flag para saber si se está moviendo hacia una trampa

    private Vector2 currentDirection;
    //private bool isMoving = false;
    private int damage;

    [Header("Knockback")]
    public float knockbackDuration = 0.2f; // Duraci�n del desplazamiento
    public int knockbackTiles = 5; // Celdas a retroceder
    private bool isKnockbackActive = false;

    [Header("Trampas")]
    [SerializeField] private LayerMask trapLayer; // TRAMPERO: capa de las trampas
    [SerializeField] private float trapDetectionDistance = 1.2f; // TRAMPERO: distancia para detectar trampas en el camino

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Transform>();
        StartCoroutine(MoveRoutine());
    }

    public void RestartCoroutineMove()
    {
        StartCoroutine(MoveRoutine());
    }

    public void StopCoroutineMove()
    {
        StopCoroutine(MoveRoutine());
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
        return !Physics2D.OverlapBox(nextPosition, tileSize * 0.15f, 0f, obstacleLayer);
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
        //isMoving = true;
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
        //isMoving = false;
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
        StopCoroutine(MoveRoutine()); // Detiene el movimiento normal

        Vector2 knockbackDirection = -currentDirection; // Direcci�n contraria al movimiento
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
        StartCoroutine(MoveRoutine()); // Reanuda el movimiento normal
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
