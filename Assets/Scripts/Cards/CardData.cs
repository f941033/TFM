using UnityEngine;

public enum CardType { Buff, Trap, DeckEffect }

public abstract class CardData : ScriptableObject
{
    [Header("Datos comunes")]
    public string cardName;
    public float cost;
    [TextArea] public string description;
    public CardType cardType;

    public abstract void Play(PlayerController player, Vector3 worldPosition);
}