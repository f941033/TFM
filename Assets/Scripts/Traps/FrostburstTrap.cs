using UnityEngine;

public class FrostburstTrap : MonoBehaviour
{
    [Header("Activación")]
    public float triggerDelay = 1.5f;     // segundos desde que detecta al primer enemigo hasta que explota
    private bool triggered = false;

    [Header("Explosión")]
    public float explosionRadius = 1.4f;  // aprox. 3 tiles según tu grid
    public float freezeDuration = 2f;     // cuánto tiempo quedan congelados
    public LayerMask enemyLayer;

    [Header("FX opcional")]
    public GameObject explosionFxPrefab;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        EnemyMovement enemy = other.GetComponent<EnemyMovement>();
        if (enemy != null)
        {
            triggered = true;
            StartCoroutine(ExplosionRoutine());
        }
    }

    private System.Collections.IEnumerator ExplosionRoutine()
    {
        yield return new WaitForSeconds(triggerDelay);

        if (explosionFxPrefab != null)
        {
            Instantiate(explosionFxPrefab, transform.position, Quaternion.identity);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            EnemyMovement enemy = hit.GetComponent<EnemyMovement>();
            if (enemy != null)
            {
                enemy.Freeze(freezeDuration);
            }
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
