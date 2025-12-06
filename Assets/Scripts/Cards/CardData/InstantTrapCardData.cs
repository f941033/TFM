using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Cards/InstantTrapCardData")]
public class InstantTrapCardData : TrapCardData
{
    //public LayerMask obstacleLayers; // Asigna las capas de obst�culos REALES (ej: "Walls", "Default")
    public override void Play(PlayerController player, Vector3 worldPosition)
    {
        var trap = Instantiate(trapPrefab, worldPosition, Quaternion.identity);
        var trapCon = trap.GetComponent<TrapController>();
        trapCon.cardData = this;
        trapCon.player = player;

    }

    public override void OnTrigger(PlayerController player, EnemyController enemy)
    {
        enemy.ReceiveDamage(damage);
        enemy.ReduceSpeed(reduceSpeed, seconds);

        if(uses == 1)
        {
            used = true;
        }
    }
}
