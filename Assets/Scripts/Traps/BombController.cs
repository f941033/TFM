using UnityEngine;

public class BombController : MonoBehaviour
{
    public ParticleSystem explosionEffect;
    public float explosionRadius = 4f;

    Animator animator;

    public float rangoDeteccion = 2.0f;
    private bool activada = false;
    public int damage;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        if (activada) return; // Evita reactivar si ya está activada

        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemigo in enemigos)
        {
            float distancia = Vector2.Distance(transform.position, enemigo.transform.position);
            if (distancia <= rangoDeteccion)
            {
                ActivarTrampa();
                break;
            }
        }
    }

    void ActivarTrampa()
    {
        activada = true;
        animator.SetTrigger("explode");
        Invoke("Explode", 0.75f);
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

        //Aplicar daño en área
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            explosionRadius
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                hit.GetComponent<EnemyMovement>().ApplyKnockback(damage);
            }
        }

        Destroy(gameObject); // Destruir la bomba
    }


}
