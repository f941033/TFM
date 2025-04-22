using UnityEngine;

public abstract class TrapCardData : CardData
{
    public bool used = false;
    public GameObject trapPrefab;
    public int damage;
    private void OnEnable() { cardType = CardType.Trap; }

    public abstract void OnTrigger(PlayerController player, EnemyController enemy);
}
