using UnityEngine;

[CreateAssetMenu(menuName = "Cards/DeckEffectCard")]
public class DeckEffectCardData : CardData
{
    public enum Effect { DrawFromDiscard, HandSizeBuff, Draw, LastUsed }
    public Effect effectType;
    public byte count;
    public float duration;
    public int cost;

    private void OnEnable() => cardType = CardType.DeckEffect;

    public override void Play(PlayerController player, Vector3 worldPos)
    {
        var cardManager = FindFirstObjectByType<CardManager>();
        switch (effectType)
        {
            case Effect.DrawFromDiscard:
                cardManager.DrawFromDiscard(count);
                break;
            case Effect.HandSizeBuff:
                //cardManager.IncereaseHandSizeTemporary(count, duration);
                break;
            case Effect.Draw:
                Debug.Log("Entro el deckEffectCardData de robar");
                for (int i = 0; i < count; i++)
                    cardManager.DrawCard();
                break;
            case Effect.LastUsed:
                Debug.Log("Entro el deckEffectCardData");
                for (int i = 0; i < count; i++)
                    cardManager.DrawLastCardUsed();
                break;
        }
    }
    
}