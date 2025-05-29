using System.Collections.Generic;
using UnityEngine;

public class TorretaController : MonoBehaviour
{
    [Header("Configuración")]
    public float fireRate = 1f;
    public float detectionRadius = 1f;
    public GameObject projectilePrefab;
    public float speed = 5f;

    private List<Transform> enemiesInRange = new List<Transform>();
    private float fireCountdown = 0f;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (enemiesInRange.Count > 0)
        {
            if (fireCountdown <= 0f)
            {
                Shoot();
                fireCountdown = 1f / fireRate;
            }
            fireCountdown -= Time.deltaTime;
        }
    }

    void Shoot()
    {
        if (projectilePrefab != null && enemiesInRange.Count > 0)
        {
            audioSource.Play();
            // Instanciar proyectil en el pivote de la torreta, sin rotación
            GameObject projectile = Instantiate(
                projectilePrefab,
                transform.position,
                Quaternion.identity
            );

            // Dirección hacia el primer enemigo en rango
            Transform target = enemiesInRange[0];
            Vector2 direction = (target.position - transform.position).normalized;

            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * speed; 
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && !enemiesInRange.Contains(other.transform))
        {
            enemiesInRange.Add(other.transform);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && enemiesInRange.Contains(other.transform))
        {
            enemiesInRange.Remove(other.transform);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
