using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    NavMeshAgent agent;
    Transform target;
    [SerializeField] private float health = 50f;
    [SerializeField] private float currentHealth;
    private float damage = 10f;
    private float attackCooldown = 0f;
    public float attackRate = 1f;
    public float attackRange = 1f;
    private bool  playerInRange = false;
    public int gold;

    void Awake(){
        currentHealth = health;
        gold = 10;
    }

    public void receiveDamage(float damage){
        currentHealth -= damage;
        if(currentHealth <=0 ){
            Die();
        }
    }
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        target = GameObject.Find("Player").transform;

        agent.updateRotation = false;
        agent.updateUpAxis = false;
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
        agent.SetDestination(target.position);

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
            player.receiveDamage(damage);
        }
    }

    private void Die(){
        PlayerController player = target.GetComponent<PlayerController>();
        player?.AddGold(gold);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        var col = GetComponent<CircleCollider2D>();
        if (col == null) return;

        // El centro del círculo respecto al world
        Vector3 center = transform.position + (Vector3)col.offset;
        float   radius = col.radius;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, radius);
    }
}
