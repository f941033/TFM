using UnityEngine;
using System.Collections;

public class HeroeMovement : MonoBehaviour
{
    Animator m_Animator;
    private EnemyMovement movimientoScript;
    float previousSpeed;

    private void Start()
    {
        m_Animator = GetComponent<Animator>();
        movimientoScript = GetComponent<EnemyMovement>();
        previousSpeed = movimientoScript.moveSpeed;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "PlayerWall")
        {
            Debug.Log("detectando caja");
            m_Animator.SetBool("attacking", true);
            StartCoroutine("DestroyBox", collision);
        }
    }

    IEnumerator DestroyBox(Collision2D collision)
    {
        // Detener movimiento completamente
        movimientoScript.StopCoroutineMove();
        movimientoScript.moveSpeed = 0;

        DestructibleWall box = collision.gameObject.GetComponent<DestructibleWall>();
        
        while (box.health>0)        // golpea hasta destruir
        {
            box.TakeDamage(1);
            yield return new WaitForSeconds(1);
        }

        Debug.Log("muro destruido");

        yield return null;            // ► espera un frame: el collider ya no existe
        yield return null;

        m_Animator.SetBool("attacking", false);
        movimientoScript.moveSpeed = previousSpeed;

        // ► forzar nuevo path al player
        movimientoScript.ClearAndRepath();
    }
}
