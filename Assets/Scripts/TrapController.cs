using UnityEngine;
using DeckboundDungeon.Cards;
public class TrapController : MonoBehaviour
{
    [HideInInspector] public TrapCardData cardData;
    [HideInInspector] public PlayerController player;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        var enemy = other.GetComponent<EnemyController>();
        if (enemy == null) return;

        // Le decimos a la carta que aplique su lógica de trigger
        cardData.OnTrigger(player, enemy);
        Debug.Log("Ha encontrado un enemigo");

        if (cardData.used)
            Destroy(gameObject);

    }
}