using UnityEngine;

[CreateAssetMenu(menuName = "Cards/DeckEffectCard")]
public class DeckEffectCardData : CardData
{
    public enum Effect { DrawFromDiscard, HandSizeBuff, Draw }
    public Effect effectType;
    public byte count;
    public float duration;

    private void OnEnable() => cardType = CardType.DeckEffect;

    public override void Play(PlayerController player, Vector3 worldPos)
    {
        var cardManager = FindFirstObjectByType<CardManager>();
        switch (effectType)
        {
            case Effect.DrawFromDiscard:
                Debug.Log("Entro en el case de la cardeffect de discard");
                cardManager.DrawFromDiscard(count);
                break;
            case Effect.HandSizeBuff:
                //cardManager.IncereaseHandSizeTemporary(count, duration);
                break;
            case Effect.Draw:
                Debug.Log("Entro en el case de la cardeffect de robar");
                for (int i = 0; i < count; i++)
                    cardManager.DrawCard();
                break;
        }
    }
    
}