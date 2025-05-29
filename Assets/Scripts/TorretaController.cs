using System.Collections.Generic;
using UnityEngine;

public class TorretaController : MonoBehaviour
{
    [Header("Configuraci�n")]
    public float rotationSpeed = 5f;
    public float fireRate = 1f;
    public float detectionRadius = 10f;
    public GameObject projectilePrefab;
    public Transform firePoint; // Objeto hijo del ca��n donde salen los proyectiles

    private List<Transform> enemiesInRange = new List<Transform>();
    private Transform currentTarget;
    private float fireCountdown = 0f;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void Update()
    {
        FindNearestTarget();
        AimAndShoot();
    }

    void FindNearestTarget()
    {
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (Transform enemy in enemiesInRange)
        {
            if (enemy == null) continue;

            // Usamos posici�n del padre (base) para c�lculo de distancias
            float distance = Vector3.Distance(transform.parent.position, enemy.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        currentTarget = nearestEnemy;
    }

    void AimAndShoot()
    {
        if (currentTarget != null)
        {
            // Rotaci�n del ca��n (hijo) respecto a la base (padre)
            Vector3 direction = (currentTarget.position + Vector3.up * 0.5f) - transform.parent.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

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
        if (projectilePrefab != null && firePoint != null)
        {
            audioSource.Play();
            GameObject projectile = Instantiate(
                projectilePrefab,
                firePoint.position,
                firePoint.rotation
            );

            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Direcci�n basada en la rotaci�n del ca��n
                rb.linearVelocity = firePoint.up * 10f;
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

}
