using UnityEngine;
[CreateAssetMenu(menuName="Shop/Card Item")]
public class CardItem : MerchantItem {
  public CardData cardData;
  public override void Apply(PlayerController player, GameManager game) {
    game.AddCardToDeck(cardData);
  }
}