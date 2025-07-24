using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]

public class MinionController : MonoBehaviour
{
    [Header("Deteccion")]
    [SerializeField] float detectionRadius = 8f;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] LayerMask obstacleLayer; 

    [Header("Movimiento y Navegacion")]
    [SerializeField] float moveSpeed = 2.5f;
    [SerializeField] float attackDistance;
    [SerializeField] float avoidanceDistance = 1.2f; // Distancia para evitar obstaculos
    [SerializeField] int raycastCount = 5; // Numero de rayos para detecci�n

    [Header("Combate")]
    [SerializeField] int damage = 25;
    [SerializeField] float timeBetweenHits = 1.1f;

    [Header("Vida")]
    [SerializeField] int maxHealth = 100;

    // Estados de la IA
    enum State { Idle, Chase, Attack, Return, Dead }
    State currentState = State.Idle;

    // Componentes
    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer spriteRenderer;

    // Variables internas
    Transform currentTarget;
    Vector3 originPoint;
    int currentHealth;
    bool canHit = true;
    bool isFacingRight = true; // El sprite originalmente mira a la derecha

    // Variables para obstacle avoidance
    Vector2 avoidanceDirection = Vector2.zero;
    float avoidanceCooldown = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        originPoint = transform.position;
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (currentState == State.Dead) return;

        // Buscar objetivos si no tenemos uno
        if (currentTarget == null)
            FindTarget();

        // Maquina de estados principal
        switch (currentState)
        {
            case State.Idle:
                HandleIdleState();
                break;
            case State.Chase:
                HandleChaseState();
                break;
            case State.Attack:
                HandleAttackState();
                break;
            case State.Return:
                HandleReturnState();
                break;
        }

        // Reducir cooldown de avoidance
        if (avoidanceCooldown > 0)
            avoidanceCooldown -= Time.deltaTime;
    }

    void HandleIdleState()
    {
        anim.ResetTrigger("run");
        anim.SetTrigger("idle");

        if (currentTarget != null)
            currentState = State.Chase;
    }

    void HandleChaseState()
    {
        if (currentTarget == null)
        {
            currentState = State.Return;
            return;
        }

        anim.ResetTrigger("idle");
        anim.SetTrigger("run");

        Vector2 directionToTarget = ((Vector2)currentTarget.position - (Vector2)transform.position).normalized;
        Vector2 finalDirection = CalculateMovementDirection(directionToTarget);

        // Mover el personaje
        MoveCharacter(finalDirection);

        // Voltear sprite seg�n la direcci�n de movimiento
        FlipSpriteBasedOnDirection(finalDirection);

        // Verificar si llegamos a distancia de ataque
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        if (distanceToTarget <= attackDistance)
        {
            rb.linearVelocity = Vector2.zero;
            currentState = State.Attack;
        }
    }

    void HandleAttackState()
    {
        if (currentTarget == null)
        {
            currentState = State.Return;
            return;
        }

        // Mirar hacia el objetivo
        Vector2 directionToTarget = ((Vector2)currentTarget.position - (Vector2)transform.position).normalized;
        FlipSpriteBasedOnDirection(directionToTarget);

        if (canHit)
            StartCoroutine(PerformAttack());

        // Si el enemigo se aleja, volver a perseguir
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        if (distanceToTarget > attackDistance + 0.3f)
            currentState = State.Chase;
    }

    void HandleReturnState()
    {
        anim.ResetTrigger("idle");
        anim.SetTrigger("run");

        Vector2 directionToOrigin = ((Vector2)originPoint - (Vector2)transform.position).normalized;
        Vector2 finalDirection = CalculateMovementDirection(directionToOrigin);

        MoveCharacter(finalDirection);
        FlipSpriteBasedOnDirection(finalDirection);

        if (Vector2.Distance(transform.position, originPoint) < 0.2f)
            currentState = State.Idle;
    }

    Vector2 CalculateMovementDirection(Vector2 preferredDirection)
    {
        // Si estamos en cooldown de avoidance, usar la direcci�n de avoidance
        if (avoidanceCooldown > 0 && avoidanceDirection != Vector2.zero)
            return avoidanceDirection.normalized;

        // Verificar obst�culos usando m�ltiples rayos
        Vector2 avoidDirection = CheckForObstacles(preferredDirection);

        if (avoidDirection != Vector2.zero)
        {
            avoidanceDirection = avoidDirection;
            avoidanceCooldown = 0.5f; // Mantener direcci�n de avoidance por medio segundo
            return avoidDirection.normalized;
        }

        return preferredDirection;
    }

    Vector2 CheckForObstacles(Vector2 preferredDirection)
    {
        float angleStep = 45f; // �ngulo entre rayos
        float rayDistance = avoidanceDistance;

        // Verificar el rayo central (direcci�n preferida)
        RaycastHit2D centerHit = Physics2D.Raycast(transform.position, preferredDirection, rayDistance, obstacleLayer);

        if (centerHit.collider != null)
        {
            // Hay obst�culo en la direcci�n preferida, buscar alternativas
            Vector2 bestDirection = Vector2.zero;
            float bestScore = -1f;

            // Probar direcciones alternativas
            for (int i = 1; i <= raycastCount / 2; i++)
            {
                // Probar a la derecha
                Vector2 rightDir = RotateVector(preferredDirection, angleStep * i);
                RaycastHit2D rightHit = Physics2D.Raycast(transform.position, rightDir, rayDistance, obstacleLayer);

                if (rightHit.collider == null)
                {
                    float score = Vector2.Dot(rightDir, preferredDirection); // Qu� tan cerca est� de la direcci�n preferida
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestDirection = rightDir;
                    }
                }

                // Probar a la izquierda  
                Vector2 leftDir = RotateVector(preferredDirection, -angleStep * i);
                RaycastHit2D leftHit = Physics2D.Raycast(transform.position, leftDir, rayDistance, obstacleLayer);

                if (leftHit.collider == null)
                {
                    float score = Vector2.Dot(leftDir, preferredDirection);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestDirection = leftDir;
                    }
                }
            }

            return bestDirection;
        }

        return Vector2.zero; // No hay obst�culos
    }

    Vector2 RotateVector(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
    }

    void MoveCharacter(Vector2 direction)
    {
        Vector2 movement = direction * moveSpeed * Time.deltaTime;

        // Usar MovePosition para respetar colisiones autom�ticamente
        rb.MovePosition((Vector2)transform.position + movement);
    }

    void FlipSpriteBasedOnDirection(Vector2 direction)
    {
        // Solo voltear si hay movimiento horizontal significativo
        if (Mathf.Abs(direction.x) > 0.1f)
        {
            bool shouldFaceRight = direction.x > 0;

            if (shouldFaceRight != isFacingRight)
            {
                isFacingRight = shouldFaceRight;
                spriteRenderer.flipX = !isFacingRight; // flipX invierte el sprite
            }
        }
    }

    void FindTarget()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position,detectionRadius,enemyLayer);
        if (hit != null && hit.CompareTag("Enemy"))
        {
            currentTarget = hit.transform;
        }
    }

    IEnumerator PerformAttack()
    {
        canHit = false;
        anim.ResetTrigger("run");
        anim.SetTrigger("attack");

        yield return new WaitForSeconds(0.35f); // Ventana de impacto

        if (currentTarget != null && currentState == State.Attack)
        {
            //IDamageable damageable = currentTarget.GetComponent<IDamageable>();
            //damageable?.TakeDamage(damage);
            currentTarget.GetComponent<EnemyController>().ReceiveDamage(damage);
        }

        yield return new WaitForSeconds(timeBetweenHits);
        canHit = true;
    }

    public void TakeDamage(int amount)
    {
        if (currentState == State.Dead) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
            StartCoroutine(Die());
    }

    IEnumerator Die()
    {
        currentState = State.Dead;
        rb.linearVelocity = Vector2.zero;
        //anim.ResetTrigger("run");
        //anim.SetTrigger("Die");

        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }


}
