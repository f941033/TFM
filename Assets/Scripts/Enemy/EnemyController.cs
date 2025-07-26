using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using static UnityEngine.GraphicsBuffer;

public class EnemyController : MonoBehaviour
{
    public enum EnemyType { Aventurero, Zapador, Heroe, Paladin, Mago, Destructor }

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
    private float attackRange = 10f;
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

        // Distancia al objetivo dinámico
        float distance = Vector2.Distance(transform.position, currentTarget.position);

        // El ataque se realiza si está cerca del objetivo
        if ((attackingMinion || playerInRange) && distance <= currentAttackRange && attackCooldown <= 0f)
        {
            Attack();
            attackCooldown = 1f / attackRate;
        }        

        FlipSprite();
    }

    private void Attack()
    {
        if (currentTarget == null) return;

        audioSource.PlayOneShot(attackSound);
        animator.SetBool("attacking", true);
        GetComponent<EnemyMovement>().moveSpeed = 0;

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
        if (!attackingMinion && minion != null)
        {
            currentTarget = minion.transform;
            minionTarget = minion;
            attackingMinion = true;       
        }
    }

    // Cuando el minion muere
    public void GoBackToPlayer()
    {
        currentTarget = playerTarget;
        attackingMinion = false;
        minionTarget = null;
        animator.SetBool("attacking", false);
        GetComponent<EnemyMovement>().moveSpeed = moveSpeed;
    }

    public void ReceiveDamage(float damage)
    {
        currentHealth -= damage;
        healthBarUI.fillAmount = currentHealth / health;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
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
