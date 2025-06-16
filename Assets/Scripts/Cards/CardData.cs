using UnityEngine;

public enum CardType { Buff, Trap, DeckEffect }

public abstract class CardData : ScriptableObject
{
    [Header("Datos comunes")]
    public string cardName;
    [TextArea] public string description;
    public CardType cardType;
    public float healthModifier;
    public int numberOfTiles;
    public int goldCost;

    public abstract void Play(PlayerController player, Vector3 worldPosition);
}