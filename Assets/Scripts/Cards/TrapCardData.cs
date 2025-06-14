using UnityEngine;
using UnityEngine.Rendering;

public abstract class TrapCardData : CardData
{
    public bool used = false;
    public int uses;
    public GameObject trapPrefab;
    public float cost;
    public int damage;
    public Vector2Int trapSizeInTiles = Vector2Int.one;

    [Header ("Reduce speed")]
    public float reduceSpeed;
    public int seconds;

    private void OnEnable() { cardType = CardType.Trap; }   

    public abstract void OnTrigger(PlayerController player, EnemyController enemy);
}
