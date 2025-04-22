using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Cards/ContiniousTrapCardData")]
public class ContiniousTrapCardData : TrapCardData
{
    [Header("Cloud Settings")]
    public float duration;
    public float radius;

    public override void Play(PlayerController player, Vector3 worldPosition)
    {
        player.StartCoroutine(ContiniousCoroutine(player.transform.position));
    }

    public override void OnTrigger(PlayerController player, EnemyController enemy)
    {
        enemy.receiveDamage(damage * Time.deltaTime);
    }

    private IEnumerator ContiniousCoroutine(Vector3 center)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            var hits = Physics2D.OverlapCircleAll(center, radius);
            foreach (var c in hits)
            {
                if (c.CompareTag("Enemy"))
                    c.GetComponent<EnemyController>().receiveDamage(damage * Time.deltaTime);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
