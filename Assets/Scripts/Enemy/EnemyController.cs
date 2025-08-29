using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using static UnityEngine.GraphicsBuffer;

public class EnemyController : MonoBehaviour
{
    public enum EnemyType { Aventurero, Zapador, Heroe, Paladin, Mago, Trampero }

    public EnemyType type;
    Transform playerTarget;
    Transform currentTarget;            // objetivo actual (Player o Minion)
    MinionController2 minionTarget;     // referencia fuerte si atacando minion
    bool attackingMinion = false;

    [Header("Configuración Genérica")]
    [SerializeField] private float health;
    [SerializeField] private float currentHealth;
    [SerializeField] private float damage = 10f;
    [SerializeField] private ParticleSystem particlesAttack;
    private float originalDamage;
    private float attackCooldown = 0f;
    private float attackRate = 1.5f;
    public float currentAttackRate;
    private float attackRange = 1f;
    public float currentAttackRange;
    private bool playerInRange = false;
    public int gold;
    private Animator animator;
    private AudioSource audioSource;
    public Image healthBarUI;
    private SpriteRenderer spriteRenderer;

    [Header("Audio")]
    public AudioClip attackSound;
    public AudioClip trapAttackSound;
    public GameObject effectGoldPrefab;

    private Coroutine debuffCoroutine;
    private float moveSpeed;

    [Header("Configuración Trampero")]
    [SerializeField] private float trapAttackRate = 0.8f;      // velocidad de ataque a trampas
    [SerializeField] private int damageTrap;
    [SerializeField] private ParticleSystem particlesTrampero;
    private Coroutine trapAttackCoroutine;  // corrutina para ataque continuo
    bool attackingTrap = false;
    Transform trapTarget;

    [Header("Configuración Modo Emergencia")]
    [SerializeField] private int damageWall = 1; // daño que hace a los muros en modo emergencia
    [SerializeField] private float wallAttackRate = 1f; // velocidad de ataque a muros
    private Coroutine wallAttackCoroutine;
    bool attackingWall = false;
    Transform wallTarget;

    void Awake()
    {
        currentHealth = health;
        currentAttackRate = attackRate;
        currentAttackRange = attackRange;
        originalDamage = damage;
    }

    void Start()
    {
        playerTarget = GameObject.Find("Player").transform;
        currentTarget = playerTarget; // objetivo inicial
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        moveSpeed = GetComponent<EnemyMovement>().moveSpeed;
    }

    void Update()
    {
        if (currentHealth <= 0) return;

        UpdateFacingDirection();

        // 1) Si atacando minion
        if (attackingMinion)
        {
            // Si minion murió, volver a Walk
            if (minionTarget == null || minionTarget.currentHealth <= 0)
            {
                attackingMinion = false;
                animator.SetBool("attacking", false);
                GetComponent<EnemyMovement>().ClearAndRepath();
                return;
            }

            // Atacar mientras esté en rango
            float dist = Vector2.Distance(transform.position, minionTarget.transform.position);
            if (dist <= currentAttackRange)
            {
                if (attackCooldown <= 0f)
                {
                    Attack();
                    attackCooldown = 1f / currentAttackRate;
                }
            }
            else
            {
                // Si el minion se aleja, replantear camino hacia él
                GetComponent<EnemyMovement>().ClearAndRepath();
            }
            return;
        }

        // 2) Si el minion murió, volver a Walk
        if (attackingMinion && (minionTarget == null || minionTarget.currentHealth <= 0))
        {
            attackingMinion = false;
            animator.SetBool("attacking", false);
            GetComponent<EnemyMovement>().ClearAndRepath();
        }



        if (currentTarget == null && !attackingWall)
        {
            GoBackToPlayer();
        }

        // Controla el tiempo entre ataques
        if (attackCooldown > 0f)
            attackCooldown -= Time.deltaTime;

        // NO atacar trampas ni muros desde Update, eso se maneja con corrutina
        if (!attackingTrap && !attackingWall)
        {
            // Distancia al objetivo dinámico
            float distance = Vector2.Distance(transform.position, currentTarget.position);

            // El ataque se realiza si está cerca del objetivo
            if ((attackingMinion || playerInRange) && distance <= currentAttackRange && attackCooldown <= 0f)
            {
                Attack();
                attackCooldown = 1f / attackRate;
            }
        }
        //FlipSprite();
    }

    private bool facingRight = true;  // Track de la última orientación horizontal

    private void UpdateFacingDirection()
    {
        if (currentTarget == null) return;

        float dx = currentTarget.position.x - transform.position.x;

        if (Mathf.Abs(dx) > 0.01f)  // Solo cambiar cuando hay diferencia horizontal significativa
        {
            facingRight = dx > 0;
            spriteRenderer.flipX = !facingRight;
            if (particlesTrampero != null)
            {
                if (facingRight)
                {
                    ParticleSystem ps = particlesTrampero.GetComponentInChildren<ParticleSystem>();
                    var main = ps.main;
                    main.startRotation = 155 * Mathf.Deg2Rad;
                    particlesTrampero.transform.localPosition = new Vector3(0.446f, 0.27f, 0f);
                }

                else
                {
                    ParticleSystem ps = particlesTrampero.GetComponentInChildren<ParticleSystem>();
                    var main = ps.main;
                    main.startRotation = -30 * Mathf.Deg2Rad;
                    particlesTrampero.transform.localPosition = new Vector3(-0.363f, 0.27f, 0f);
                }

            }
        }
        // Si dx es casi 0 (objetivo arriba/abajo), no tocar flipX: mantiene la última orientación
    }



    private void Attack()
    {
        if (currentTarget == null) return;

        if(attackSound != null) audioSource.PlayOneShot(attackSound);
        animator.SetBool("attacking", true);
        GetComponent<EnemyMovement>().moveSpeed = 0;

        if (minionTarget != null)
        {
            minionTarget.ReceiveDamage(damage);
            Debug.Log($"[{gameObject.name}] Atacando minion: daño {damage}");

            // Si el minion muere con este ataque, limpiar estado
            if (minionTarget.currentHealth <= 0)
            {
                Debug.Log($"[{gameObject.name}] Minion muerto. Volviendo al player.");
                GoBackToPlayer();
                return;
            }
        }
        else
        {
            // Ataca al Player
            var damageable = currentTarget.GetComponent<Damageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                currentTarget.GetComponent<PlayerController>().receiveDamage(damage);
            }
        }

        StartCoroutine(ResetAttackAnimation());
    }

    private IEnumerator ResetAttackAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("attacking", false);
        GetComponent<EnemyMovement>().moveSpeed = moveSpeed;

        // Si estábamos atacando un minion pero éste ya no existe, volver al player
        if (attackingMinion && (minionTarget == null || minionTarget.currentHealth <= 0))
        {
            Debug.Log($"[{gameObject.name}] Finalizada animación de ataque y minion muerto. Retomando camino al player.");
            GoBackToPlayer();
        }
    }


    // TRAMPERO: Método para iniciar ataque continuo a trampa
    public void StartTrapAttack()
    {
        if (trapAttackCoroutine != null)
            StopCoroutine(trapAttackCoroutine);

        trapAttackCoroutine = StartCoroutine(ContinuousTrapAttack());
    }

    // TRAMPERO: Corrutina para atacar trampa continuamente
    private IEnumerator ContinuousTrapAttack()
    {
        particlesTrampero.Play();
        animator.SetBool("attacking", true);
        GetComponent<EnemyMovement>().moveSpeed = 0;

        while (attackingTrap && trapTarget != null)
        {
            // Verificar si la trampa sigue existiendo
            var trapComponent = trapTarget.GetComponent<CardBehaviour>();
            if (trapComponent == null)
            {
                // La trampa fue destruida
                break;
            }

            // Reproducir sonido y atacar
            if (audioSource != null && trapAttackSound != null)
                audioSource.PlayOneShot(trapAttackSound);

            trapComponent.ReceiveDamage(damageTrap);

            // Esperar antes del siguiente ataque
            yield return new WaitForSeconds(1f / trapAttackRate);

            // Verificar de nuevo si la trampa fue destruida después del ataque
            if (trapTarget == null || trapTarget.gameObject == null)
            {
                break;
            }
        }

        // Ataque terminado, volver al comportamiento normal
        particlesTrampero.Stop();
        GoBackToPlayer();
        trapAttackCoroutine = null;
    }

    // MODO EMERGENCIA: Método para notificar que se encontró un muro que atacar
    public void NotifyWallFound(Transform wall)
    {
        if (!attackingMinion && !attackingTrap && wall != null)
        {
            currentTarget = wall;
            wallTarget = wall;
            attackingWall = true;
            // Iniciar ataque continuo al muro
            StartWallAttack();
        }
    }

    // MODO EMERGENCIA: Método para iniciar ataque continuo a muro
    public void StartWallAttack()
    {
        if (wallAttackCoroutine != null)
            StopCoroutine(wallAttackCoroutine);

        wallAttackCoroutine = StartCoroutine(ContinuousWallAttack());
    }

    // MODO EMERGENCIA: Corrutina para atacar muro continuamente
    private IEnumerator ContinuousWallAttack()
    {
        animator.SetBool("attacking", true);
        GetComponent<EnemyMovement>().moveSpeed = 0;

        while (attackingWall && wallTarget != null)
        {
            // Verificar antes de cada ataque si hay camino al player
            var movement = GetComponent<EnemyMovement>();
            var path = movement.FindPathBFS(
                movement.SnapToGrid(transform.position),
                movement.SnapToGrid(playerTarget.position)
            );
            if (path != null && path.Count > 0)
            {
                // ¡Camino abierto! Salir del modo emergencia
                attackingWall = false;
                Debug.Log($"[{gameObject.name}] Camino al player disponible, deteniendo ataque a muros.");
                break;
            }

            // Si llega aquí, el path sigue bloqueado → atacar muro
            var wallComponent = wallTarget.GetComponent<DestructibleWall>();
            if (wallComponent == null) break;
            if(attackSound != null) audioSource.PlayOneShot(attackSound);
            wallComponent.TakeDamage(damageWall);

            yield return new WaitForSeconds(1f / wallAttackRate);
        }

        // Limpieza tras terminar ataque (ya sea por muro destruido o path disponible)
        GetComponent<EnemyMovement>().OnWallDestroyed();
        GoBackToPlayer();
        wallAttackCoroutine = null;
    }


    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInRange = true;
            attackCooldown = 1f / attackRate;
        }

        if (col.CompareTag("DeathZone"))
        {
            Die();
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            playerInRange = false;
    }

    // Llamar desde Minion al recibir daño si no se usan triggers: enemyController.NotifyAttackedByMinion(this)
    public void NotifyAttackedByMinion(MinionController2 minion)
    {
        if (minion == null) return;

        // Cambiar objetivo
        currentTarget = minion.transform;
        minionTarget = minion;
        attackingMinion = true;

        // Orientación y animación de ataque
        UpdateFacingDirection();
        animator.SetBool("attacking", true);

        // Parar movimiento
        var movement = GetComponent<EnemyMovement>();
        movement.StopCoroutineMove();
        movement.moveSpeed = 0f;

        // Resetear cooldown para ataque inmediato
        attackCooldown = 0f;

        Debug.Log($"[{gameObject.name}] Atacado por minion, entrando en ATTACK.");
    }





    // TRAMPERO: Método para notificar que se encontró una trampa en el camino
    public void NotifyTrapFound(Transform trap)
    {
        // Solo el enemigo Trampero puede atacar trampas
        if (type != EnemyType.Trampero) return;

        // Solo cambiar objetivo si no está atacando un minion o muro
        if (!attackingMinion && !attackingWall && trap != null)
        {
            currentTarget = trap;
            trapTarget = trap;
            attackingTrap = true;
            // Iniciar ataque continuo a la trampa
            StartTrapAttack();
        }
    }

    // Cuando el minion muere o se termina cualquier ataque especial
    public void GoBackToPlayer()
    {
        // Detener ataques activos
        if (trapAttackCoroutine != null)
        {
            StopCoroutine(trapAttackCoroutine);
            trapAttackCoroutine = null;
        }
        if (wallAttackCoroutine != null)
        {
            StopCoroutine(wallAttackCoroutine);
            wallAttackCoroutine = null;
        }

        // Resetear estados
        currentTarget = playerTarget;
        attackingMinion = false;
        attackingTrap = false;
        attackingWall = false;
        minionTarget = null;
        trapTarget = null;
        wallTarget = null;
        if(particlesTrampero != null) { particlesTrampero.Stop(); }

        // Resetear animación y movimiento
        animator.SetBool("attacking", false);
        GetComponent<EnemyMovement>().moveSpeed = moveSpeed;
        GetComponent<EnemyMovement>().ClearTrapTarget();
        GetComponent<EnemyMovement>().ClearAndRepath();
    }

    public void ReceiveDamage(float damage)
    {
        currentHealth -= damage;
        healthBarUI.fillAmount = currentHealth / health;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        // Limpiar corrutinas antes de morir
        if (trapAttackCoroutine != null)
            StopCoroutine(trapAttackCoroutine);
        if (wallAttackCoroutine != null)
            StopCoroutine(wallAttackCoroutine);

        PlayerController player = playerTarget.GetComponent<PlayerController>();
        player?.AddGold(gold);
        gold = 0;
        Instantiate(effectGoldPrefab, transform.position, Quaternion.identity);
        FindFirstObjectByType<GameManager>().EnemyKaputt();

        Destroy(gameObject);
    }

    private void FlipSprite()
    {
        if (currentTarget == null) return;

        Vector3 direction = (currentTarget.position - transform.position).normalized;

        // Voltear horizontalmente si el objetivo está principalmente a la izquierda/derecha
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            spriteRenderer.flipX = direction.x < 0;
        }
        else
        {
            // Para objetivos verticales, opcionalmente podemos usar otro sprite o animación.
            // En este caso no invertimos horizontalmente, pero la animación de ataque seguirá.
            // Si se desea que sprite gira verticalmente, habría que usar otra propiedad.
            spriteRenderer.flipX = false;
        }
    }


    public void ReduceSpeed(float speedPercent, int seconds)
    {
        GetComponent<EnemyMovement>().ReduceSpeed(speedPercent, seconds);
    }

    public void ApplyDebuff(HabilityCardData data)
    {
        if (debuffCoroutine != null)
            StopCoroutine(debuffCoroutine);

        debuffCoroutine = StartCoroutine(HandleDebuff(data));
    }

    private IEnumerator HandleDebuff(HabilityCardData data)
    {
        foreach (var debuff in data.debuffs)
        {
            switch (debuff.type)
            {
                case DebuffType.Speed:
                    GetComponent<EnemyMovement>().ReduceSpeed(debuff.multiplier, (int)data.debuffDuration);
                    break;
                case DebuffType.Damage:
                    damage *= debuff.multiplier;
                    break;
                case DebuffType.AttackRate:
                    currentAttackRate *= debuff.multiplier;
                    break;
                case DebuffType.AttackRange:
                    currentAttackRange *= debuff.multiplier;
                    break;
            }
        }

        yield return new WaitForSeconds(data.debuffDuration);

        // Restaurar
        currentAttackRate = attackRate;
        currentAttackRange = attackRange;
        damage = originalDamage;
        debuffCoroutine = null;
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    public void ActivateParticlesAttack()
    {
        if(particlesAttack != null)
        {
            particlesAttack.Play();
            particlesAttack.GetComponent<AudioSource>().Play();
        }
            
    }
}