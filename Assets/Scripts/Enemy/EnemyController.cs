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

    [SerializeField] private float health;
    [SerializeField] private float currentHealth;
    [SerializeField] private float damage = 10f;
    private float originalDamage;
    private float attackCooldown = 0f;
    private float attackRate = 1f;
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

    public GameObject effectGoldPrefab;
    private Coroutine debuffCoroutine;
    private float moveSpeed;

    [Header("Configuración Trampero")]
    //[SerializeField] private float trapAttackRange = 0.25f;     // distancia para atacar trampas
    [SerializeField] private float trapAttackRate = 0.8f;      // velocidad de ataque a trampas
    [SerializeField] private int damageTrap;
    private Coroutine trapAttackCoroutine;  // corrutina para ataque continuo
    bool attackingTrap = false;
    Transform trapTarget;

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
        //if (!playerInRange) return;


        if (currentTarget == null)
        {
            GoBackToPlayer();
        }

        // Controla el tiempo entre ataques
        if (attackCooldown > 0f)
            attackCooldown -= Time.deltaTime;

        // NO atacar trampas desde Update, eso se maneja con corrutina
        if (!attackingTrap)
        {

            // Distancia al objetivo dinámico
            float distance = Vector2.Distance(transform.position, currentTarget.position);

            // Determinar rango de ataque según el objetivo
            //float effectiveAttackRange = attackingTrap ? trapAttackRange : currentAttackRange;

            // El ataque se realiza si está cerca del objetivo
            if ((attackingMinion || playerInRange || attackingTrap) && distance <= currentAttackRange && attackCooldown <= 0f)
            {
                Attack();
                attackCooldown = 1f / attackRate;
            }
        }
        FlipSprite();
    }

    private void Attack()
    {
        if (currentTarget == null) return;

        audioSource.PlayOneShot(attackSound);
        animator.SetBool("attacking", true);
        GetComponent<EnemyMovement>().moveSpeed = 0;

        //if (attackingTrap && trapTarget != null)
        //{
        //    // TRAMPERO: Atacar trampa
        //    var trapComponent = trapTarget.GetComponent<CardBehaviour>();
        //    if (trapComponent != null)
        //    {
        //        trapComponent.ReceiveDamage(5);

        //        // Si la trampa fue destruida, volver al comportamiento normal
        //        if (trapComponent == null)
        //        {
        //            GoBackToPlayer();
        //        }
        //    }
        //    else
        //    {
        //        // Si la trampa ya no existe, volver al player
        //        GoBackToPlayer();
        //    }
        //}
        //else

        if (minionTarget != null)
        {
            minionTarget.ReceiveDamage(damage);

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
        // Desactivar animación después de un tiempo
        //StartCoroutine(ResetAttackAnimation());
    }

    private IEnumerator ResetAttackAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("attacking", false);
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
            if (audioSource != null && attackSound != null)
                audioSource.PlayOneShot(attackSound);

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
        GoBackToPlayer();
        trapAttackCoroutine = null;
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
        if (!attackingMinion && !attackingTrap && minion != null)
        {
            currentTarget = minion.transform;
            minionTarget = minion;
            attackingMinion = true;       
        }
    }

    // TRAMPERO: Método para notificar que se encontró una trampa en el camino
    public void NotifyTrapFound(Transform trap)
    {
        // Solo el enemigo Trampero puede atacar trampas
        if (type != EnemyType.Trampero) return;

        // Solo cambiar objetivo si no está atacando un minion
        if (!attackingMinion && trap != null)
        {
            currentTarget = trap;
            trapTarget = trap;
            attackingTrap = true;
            // Iniciar ataque continuo a la trampa
            StartTrapAttack();
        }
    }

    // Cuando el minion muere
    public void GoBackToPlayer()
    {
        // Detener ataque a trampa si estaba activo
        if (trapAttackCoroutine != null)
        {
            StopCoroutine(trapAttackCoroutine);
            trapAttackCoroutine = null;
        }

        currentTarget = playerTarget;
        attackingMinion = false;
        minionTarget = null;
        attackingTrap = false;
        trapTarget = null;
        animator.SetBool("attacking", false);
        GetComponent<EnemyMovement>().moveSpeed = moveSpeed;
        GetComponent<EnemyMovement>().ClearTrapTarget();
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


        PlayerController player = playerTarget.GetComponent<PlayerController>();
        player?.AddGold(gold);
        Instantiate(effectGoldPrefab, transform.position, Quaternion.identity);
        FindFirstObjectByType<GameManager>().EnemyKaputt();

        Destroy(gameObject);
    }

    private void FlipSprite()
    {
        if (currentTarget == null) return;
        bool flip = currentTarget.position.x < transform.position.x;
        spriteRenderer.flipX = flip;
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


}
