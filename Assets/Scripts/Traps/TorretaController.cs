using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TorretaController : MonoBehaviour
{
    [Header("Configuraci�n")]
    public float rotationSpeed = 5f;
    public float fireRate = 1f;
    public float detectionRadius = 3f;
    public GameObject projectilePrefab;
    public Transform firePoint; // Objeto hijo del ca��n donde salen los proyectiles
    public TextMeshProUGUI healthText;

    [Header("Detecci�n")]
    public float checkInterval = 0.3f; // Intervalo de chequeo para optimizaci�n


    private List<Transform> enemiesInRange = new List<Transform>();
    private Transform currentTarget;
    private float fireCountdown = 0f;

    private AudioSource audioSource;
    private Animator animator;
    public int projectilesNumber = 50;
    private bool canShoot = true;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        StartCoroutine(EnemyDetectionRoutine());
        healthText.text = projectilesNumber.ToString();
    }

    IEnumerator EnemyDetectionRoutine()
    {
        while (canShoot)
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
                if (!enemiesInRange.Contains(enemy.transform))
                    enemiesInRange.Add(enemy.transform); // Añadir solo si es nuevo
            }
        }

        // 4. Actualizar lista principal. Eliminar enemigos que ya no están en rango
        enemiesInRange.RemoveAll(enemy =>
            enemy == null ||
            !currentEnemiesInRange.Contains(enemy)
        );

        // 5. A�adir nuevos enemigos
        //foreach (Transform enemy in currentEnemiesInRange)
        //{
        //    if (!enemiesInRange.Contains(enemy))
        //    {
        //        enemiesInRange.Add(enemy);
        //    }
        //}
    }

    void Update()
    {
        if (canShoot)
        {
            //FindNearestTarget();

            UpdateCurrentTarget();
            AimAndShoot();
        }
    }

    void UpdateCurrentTarget()
    {
        // El objetivo es siempre el primero que entró y sigue en rango
        if (enemiesInRange.Count > 0)
            currentTarget = enemiesInRange[0];
        else
            currentTarget = null;
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
            // Rotacion del cañon (hijo) respecto a la base (padre)
            Vector3 direction = (currentTarget.position + Vector3.up * 0.5f) - transform.parent.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
            float angleDiff = Quaternion.Angle(transform.rotation, targetRotation);
            if (angleDiff < 8f)
            {
                if (fireCountdown <= 0f)
                    {
                        StartCoroutine(Shoot());
                        fireCountdown = 1f / fireRate;
                    }   
            }
            fireCountdown -= Time.deltaTime;
        }
    }

    IEnumerator Shoot()
    {
        if (projectilePrefab != null && firePoint != null && canShoot)
        {
            animator.SetTrigger("fire");
            audioSource.Play();
            yield return new WaitForSeconds(0.35f);
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

            projectilesNumber--;
            healthText.text = projectilesNumber.ToString();
            if (projectilesNumber == 0)
            {
                canShoot = false;
                Destroy(transform.parent.gameObject);
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
