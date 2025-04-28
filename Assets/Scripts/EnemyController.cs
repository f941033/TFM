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
    public float attackRange = 1.5f;

    void Awake(){
        currentHealth = health;
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

    void Update()
    {
        agent.SetDestination(target.position);

        float distancia = Vector2.Distance(transform.position, target.position);
        if (distancia <= attackRange && attackCooldown <= 0f)
        {
            Attack();
            attackCooldown = 1f / attackRate;
        }
    }

    private void Attack()
    {
        PlayerController pc = target.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.receiveDamage(damage);
            Debug.Log("Estoy atacando");
        }
    }

    private void Die(){
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
