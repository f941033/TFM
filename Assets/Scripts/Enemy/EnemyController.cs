using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public enum EnemyType
    {
        Aventurero,
        Zapador,
        Heroe,
        Paladin,
        Mago,
        Destructor
    }
    public EnemyType type;
    Transform target;
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

    void Awake()
    {
        currentHealth = health;
        currentAttackRate = attackRate;
        currentAttackRange = attackRange;
        originalDamage = damage;
    }

    public void ReceiveDamage(float damage)
    {
        currentHealth -= damage;
        healthBarUI.fillAmount = currentHealth / health;
        GetComponent<Damageable>().TakeDamage(damage);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void ReduceSpeed(float speedPercent, int seconds)
    {
        GetComponent<EnemyMovement>().ReduceSpeed(speedPercent, seconds);
    }
    void Start()
    {
        target = GameObject.Find("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (target.position.x < transform.position.x)
        {
            spriteRenderer.flipX = true;
            //transform.localScale = new Vector3(-1,1,1);
        }
        else
        {
            spriteRenderer.flipX = false;
            //transform.localScale = new Vector3(1, 1, 1);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInRange = true;
            attackCooldown = 1f / currentAttackRate; // forzamos espera antes del primer golpe
        }
    }

    void Update()
    {

        if (!playerInRange) return;

        if (attackCooldown > 0f)
            attackCooldown -= Time.deltaTime;

        float distancia = Vector2.Distance(transform.position, target.position);
        if (distancia <= currentAttackRange && attackCooldown <= 0f)
        {
            Attack();
            attackCooldown = 1f / currentAttackRate;
        }
    }

    private void Attack()
    {
        PlayerController player = target.GetComponent<PlayerController>();
        if (player != null)
        {
            audioSource.PlayOneShot(attackSound);

            switch (type)
            {
                case EnemyType.Aventurero:
                    animator.SetTrigger("attack");
                    break;
                case EnemyType.Heroe:
                    animator.SetBool("attacking", true);
                    break;
            }

            player.receiveDamage(damage);
        }
    }

    private void Die()
    {
        PlayerController player = target.GetComponent<PlayerController>();
        player?.AddGold(gold);
        Instantiate(effectGoldPrefab, transform.position, Quaternion.identity);
        FindFirstObjectByType<GameManager>().EnemyKaputt();
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        var col = GetComponent<CircleCollider2D>();
        if (col == null) return;

        // El centro del círculo respecto al world
        Vector3 center = transform.position + (Vector3)col.offset;
        float radius = col.radius;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, radius);
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
                    damage *=debuff.multiplier;
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
