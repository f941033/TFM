using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Cards/InstantTrapCardData")]
public class InstantTrapCardData : TrapCardData
{
    //public LayerMask obstacleLayers; // Asigna las capas de obstáculos REALES (ej: "Walls", "Default")
    public override void Play(PlayerController player, Vector3 worldPosition)
    {
        /*
        // Verificar colisiones solo con obstáculos físicos (no triggers)
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            worldPosition,
            0.45f,
            obstacleLayers
        );
        if(colliders.Length == 0 )
        {
            var trap = Instantiate(trapPrefab, worldPosition, Quaternion.identity);
            var trapCon = trap.GetComponent<TrapController>();
            trapCon.cardData = this;
            trapCon.player = player;
        }*/

        var trap = Instantiate(trapPrefab, worldPosition, Quaternion.identity);
        var trapCon = trap.GetComponent<TrapController>();
        trapCon.cardData = this;
        trapCon.player = player;

    }

    public override void OnTrigger(PlayerController player, EnemyController enemy)
    {
        Debug.Log("He entrado en el trigger de instantTrap");
        enemy.ReceiveDamage(damage);
        enemy.ReduceSpeed(reduceSpeed, seconds);
        Debug.Log("He activado la trampa");

        if(uses == 1)
        {
            used = true;
        }
    }
}
