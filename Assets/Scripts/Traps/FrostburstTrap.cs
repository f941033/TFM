using UnityEngine;
using UnityEngine.UI;

public class FrostburstTrap : MonoBehaviour
{
    [Header("Activación")]
    public float triggerDelay = 1.5f;
    private bool triggered = false;

    [Header("Explosión")]
    public float explosionRadius = 1.4f;
    public float freezeDuration = 2f;
    public LayerMask enemyLayer;

    [Header("Visual")]
    public GameObject sprite;
    public Image image;

    [Header("FX opcional")]
    public GameObject explosionFxPrefab;

    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        EnemyMovement enemy = other.GetComponent<EnemyMovement>();
        if (enemy == null) return;

        triggered = true;

        image.gameObject.SetActive(true);
        StartCoroutine(ExplosionRoutine());
    }

    private System.Collections.IEnumerator ExplosionRoutine()
    {
        yield return new WaitForSeconds(triggerDelay);

        if (sprite != null)
            sprite.SetActive(false);

        if (col != null)
            col.enabled = false;

        if (explosionFxPrefab != null)
        {
            image.gameObject.SetActive(false);
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
