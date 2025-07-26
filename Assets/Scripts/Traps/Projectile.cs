using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 10;

    private void Start()
    {
        Destroy(gameObject,1f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyController>().ReceiveDamage(damage);
            Destroy(gameObject);
        }
    }
}
