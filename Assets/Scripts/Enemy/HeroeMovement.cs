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
        movimientoScript.moveSpeed = 0;
        DestructibleWall box = collision.gameObject.GetComponent<DestructibleWall>();
        int boxHealth = box.health;

        for (int i = 1; i <= boxHealth; i++)
        {
            yield return new WaitForSeconds(1);
            box.TakeDamage(1);
        }

        Debug.Log("muro destruido");
        m_Animator.SetBool("attacking", false);
        movimientoScript.moveSpeed = previousSpeed;
    }
}
