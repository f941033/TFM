using UnityEngine;

public class BombController : MonoBehaviour
{
    public ParticleSystem explosionEffect;
    public float explosionRadius = 3f;
    public int damage = 50;

    public Animator animator;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            animator.SetTrigger("explode");
            Invoke("Explode",0.75f);
        }
    }

    void Explode()
    {
        // Activar efecto de partículas
        if (explosionEffect != null)
        {
            ParticleSystem explosion = Instantiate(
                explosionEffect,
                transform.position,
                Quaternion.identity
            );
            explosion.Play();
            Destroy(explosion.gameObject, explosion.main.duration);
        }

        // Aplicar daño en área
        //Collider2D[] hits = Physics2D.OverlapCircleAll(
        //    transform.position,
        //    explosionRadius
        //);

        //foreach (Collider2D hit in hits)
        //{
        //    if (hit.CompareTag("Enemy"))
        //    {
        //        hit.GetComponent<EnemyHealth>().TakeDamage(damage);
        //    }
        //}

        //Destroy(gameObject); // Destruir la bomba
    }


}
