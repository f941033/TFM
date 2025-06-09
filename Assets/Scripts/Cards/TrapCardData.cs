using UnityEngine;

public abstract class TrapCardData : CardData
{
    public bool used = false;
    public int uses;
    public GameObject trapPrefab;
    public float cost;
    public int damage;
    private void OnEnable() { cardType = CardType.Trap; }
    public Vector2Int trapSizeInTiles = Vector2Int.one;

    public abstract void OnTrigger(PlayerController player, EnemyController enemy);
}
