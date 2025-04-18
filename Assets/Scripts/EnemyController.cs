using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    NavMeshAgent agent;
    Transform target;

    private float health = 50f;
    private float currentHealth;
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        target = GameObject.Find("Player").transform;

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(target.position);
    }

    private void Die(){
        Destroy(gameObject);
    }
}
