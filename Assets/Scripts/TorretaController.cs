using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TorretaController : MonoBehaviour
{
    [Header("Configuración")]
    public float rotationSpeed = 5f;
    public float fireRate = 1f;
    public float detectionRadius = 3f;
    public GameObject projectilePrefab;
    public Transform firePoint; // Objeto hijo del cañón donde salen los proyectiles

    [Header("Detección")]
    public float checkInterval = 0.3f; // Intervalo de chequeo para optimización


    private List<Transform> enemiesInRange = new List<Transform>();
    private Transform currentTarget;
    private float fireCountdown = 0f;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(EnemyDetectionRoutine());
    }

    IEnumerator EnemyDetectionRoutine()
    {
        while (true)
        {
            UpdateEnemiesInRange();
            yield return new WaitForSeconds(checkInterval);
        }
    }

    void UpdateEnemiesInRange()
    {
        // 1. Obtener todos los enemigos en la escena
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        // 2. Crear lista temporal de enemigos en rango
        HashSet<Transform> currentEnemiesInRange = new HashSet<Transform>();

        // 3. Chequear distancia para cada enemigo
        foreach (GameObject enemy in allEnemies)
        {
            if (enemy == null) continue;

            float distance = Vector3.Distance(
                transform.parent.position,
                enemy.transform.position
            );

            if (distance <= detectionRadius)
            {
                currentEnemiesInRange.Add(enemy.transform);
            }
        }

        // 4. Actualizar lista principal
        enemiesInRange.RemoveAll(enemy =>
            enemy == null ||
            !currentEnemiesInRange.Contains(enemy)
        );

        // 5. Añadir nuevos enemigos
        foreach (Transform enemy in currentEnemiesInRange)
        {
            if (!enemiesInRange.Contains(enemy))
            {
                enemiesInRange.Add(enemy);
            }
        }
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

            // Usamos posición del padre (base) para cálculo de distancias
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
            // Rotación del cañón (hijo) respecto a la base (padre)
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
                // Dirección basada en la rotación del cañón
                rb.linearVelocity = firePoint.up * 10f;
            }
        }
    }
    /*
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

    */
}
