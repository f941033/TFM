using UnityEngine;
using System.Collections;

public class Damageable : MonoBehaviour
{
    [Header("Invulnerabilidad")]
    public float invulDuration = 1f;
    public int flashCount = 5;

    private bool isInvulnerable = false;
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            Debug.LogError("Necesitas un SpriteRenderer en " + name);
    }

    /// <summary>
    /// Llama a este método para aplicar daño. 
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (isInvulnerable) 
            return;

        // aquí restas vida, llamas a Die() si llega a 0, etc.
        ApplyDamage(amount);

        // comienza invulnerabilidad + flash
        StartCoroutine(InvulFlash());
    }

    private void ApplyDamage(float amount)
    {
        // Ejemplo genérico: si tienes currentHealth:
        // currentHealth -= amount;
        // if (currentHealth <= 0) Die();
    }

    private IEnumerator InvulFlash()
    {
        isInvulnerable = true;
        float flashInterval = invulDuration / (flashCount * 2f);

        for (int i = 0; i < flashCount; i++)
        {
            sr.enabled = false;
            yield return new WaitForSeconds(flashInterval);
            sr.enabled = true;
            yield return new WaitForSeconds(flashInterval);
        }

        isInvulnerable = false;
    }
}
