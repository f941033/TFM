using UnityEngine;

[CreateAssetMenu(menuName = "Cards/DeckEffectCard")]
public class DeckEffectCardData : CardData
{
    /*[Header("Deck Effect")]
    public int drawCount;

    private void OnEnable() { cardType = CardType.DeckEffect; }
*/
    public override void Play(PlayerController player)
    {
        //player.DrawCards(drawCount);
    }
    
}