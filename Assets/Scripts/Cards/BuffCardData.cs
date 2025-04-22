using UnityEngine;
using DeckboundDungeon.Cards.Buff;

[CreateAssetMenu(menuName = "Cards/BuffCard")]
public class BuffCardData : CardData
{
    [Header("Buff")]
    public BuffType buffType;
    public float modifier;
    public float duration;

    private void OnEnable() { cardType = CardType.Buff; }

    public override void Play(PlayerController player, Vector3 worldPosition)
    {
        player.ApplyTemporaryBuff(buffType, modifier, duration);
    }
}