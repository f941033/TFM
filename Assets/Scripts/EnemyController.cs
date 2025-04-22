using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    NavMeshAgent agent;
    Transform target;
    [SerializeField] private float health = 50f;
    [SerializeField] private float currentHealth;
    private float damage = 10f;

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
    }

    private void Die(){
        Destroy(gameObject);
    }
}
