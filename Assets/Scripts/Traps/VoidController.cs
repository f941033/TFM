using UnityEngine;

public class VoidController : MonoBehaviour
{
    [Header("Parámetros del imán")]
    public float radius = 3f;              // radio de atracción
    public float pullDuration = 0.6f;      // cuánto tiempo dura la atracción
    public float extraSpeedMultiplier = 3f;

    [Header("Detección")]
    public LayerMask enemyLayer;           // layer de los enemigos

    void Start()
    {
        ActivateMagnet();
    }

    public void ActivateMagnet()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            EnemyMovement enemy = hit.GetComponent<EnemyMovement>();
            if (enemy != null)
            {
                enemy.ApplyMagnet(transform.position, pullDuration, extraSpeedMultiplier);
            }
        }

        Destroy(gameObject, pullDuration);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
