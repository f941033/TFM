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
    //private bool isMoving = false;
    private int damage;

    [Header("Knockback")]
    public float knockbackDuration = 0.2f; // Duraci�n del desplazamiento
    public int knockbackTiles = 5; // Celdas a retroceder
    private bool isKnockbackActive = false;

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


}
