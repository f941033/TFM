using UnityEngine;

[CreateAssetMenu(menuName = "Cards/InstantTrapCardData")]
public class InstantTrapCardData : TrapCardData
{
    public override void Play(PlayerController player, Vector3 worldPosition)
    {
        var trap = Instantiate(trapPrefab, worldPosition, Quaternion.identity);
        var trapCon = trap.GetComponent<TrapController>();
        trapCon.cardData = this;
        trapCon.player = player;
    }

    public override void OnTrigger(PlayerController player, EnemyController enemy)
    {
        Debug.Log("He entrado en el trigger de instantTrap");
        enemy.receiveDamage(damage);
        Debug.Log("He activado la trampa");
        this.used = true;
    }
}
