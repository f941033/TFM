using UnityEngine;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    [Header("Configuración")]
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
            // Calcular dirección óptima hacia el jugador
            Vector2 optimalDirection = CalculateOptimalDirection();

            // Intentar mover en dirección óptima
            if (TryMove(optimalDirection))
            {
                currentDirection = optimalDirection;
            }
            else
            {
                // Buscar dirección alternativa
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

        // Si todas las direcciones están bloqueadas, forzar movimiento
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


    
}
