using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 10;


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyController>().receiveDamage(damage);
            Destroy(gameObject);
        }
    }
}
