using UnityEngine;

public class BombController : MonoBehaviour
{
    public ParticleSystem explosionEffect;
    public float explosionRadius = 3f;
    public int damage = 50;

    public Animator animator;

    public float rangoDeteccion = 2.0f;
    private bool activada = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        if (activada) return; // Evita reactivar si ya est� activada

        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemigo in enemigos)
        {
            float distancia = Vector2.Distance(transform.position, enemigo.transform.position);
            if (distancia <= rangoDeteccion)
            {
                ActivarTrampa(enemigo);
                break;
            }
        }
    }

    void ActivarTrampa(GameObject enemigo)
    {
        activada = true;
        animator.SetTrigger("explode");
        Invoke("Explode", 0.75f);
    }


    void Explode()
    {
        // Activar efecto de part�culas
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

        //Aplicar da�o en �rea
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            explosionRadius
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                hit.GetComponent<EnemyController>().ReceiveDamage(damage);
            }
        }

        Destroy(gameObject); // Destruir la bomba
    }


}
