using UnityEngine;

public class DestructibleWall : MonoBehaviour
{
    public int health = 3;

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(gameObject);
            // Añadir efectos de destrucción aquí
        }
    }

    public bool IsDead()
    {
        return health > 0;
    }
}
