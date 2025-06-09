using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    Transform target;
    [SerializeField] private float health = 50f;
    [SerializeField] private float currentHealth;
    private float damage = 10f;
    private float attackCooldown = 0f;
    public float attackRate = 1f;
    public float attackRange = 1f;
    private bool  playerInRange = false;
    public int gold;
    private Animator animator;
    private AudioSource audioSource;
    public Image healthBarUI;
    private SpriteRenderer spriteRenderer;

    [Header("Audio")]
    public AudioClip attackSound;

    public GameObject effectGoldPrefab;

    void Awake(){
        currentHealth = health;
        gold = 20;        
    }

    public void receiveDamage(float damage){
        currentHealth -= damage;
        healthBarUI.fillAmount = currentHealth/health;
        GetComponent<Damageable>().TakeDamage(damage);
        if(currentHealth <=0 ){
            Die();
        }
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
            playerInRange   = true;
            attackCooldown  = 1f / attackRate; // forzamos espera antes del primer golpe
        }
    }

    void Update()
    {

        if(!playerInRange) return;

        if (attackCooldown > 0f)
            attackCooldown -= Time.deltaTime;

        float distancia = Vector2.Distance(transform.position, target.position);
        if (distancia <= attackRange && attackCooldown <= 0f)
        {
            Attack();
            attackCooldown = 1f / attackRate;
        }
    }

    private void Attack()
    {
        PlayerController player = target.GetComponent<PlayerController>();
        if (player != null)
        {
            audioSource.PlayOneShot(attackSound);
            animator.SetTrigger("attack");
            player.receiveDamage(damage);
        }
    }

    private void Die(){
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
}
