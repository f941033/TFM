using UnityEngine;
[CreateAssetMenu(menuName="Shop/Card Item")]
public class CardItem : MerchantItem {
  public CardData cardData;
  public override void Apply(PlayerController player, GameManager game)
  {
    game.AddCardToDeck(cardData);
  }
  public override string GetPurchaseLine(PlayerController p, GameManager g, int paidGold) {
    // si el asset tiene línea, úsala con {card}
    if (!string.IsNullOrEmpty(purchaseLine)) {
      return purchaseLine
        .Replace("{cost}", paidGold.ToString())
        .Replace("{item}", itemName)
        .Replace("{card}", cardData ? cardData.cardName : itemName);
    }

    // fallback de carta
    return paidGold > 300
      ? "A small spark; steady hands make it shine."
      : "A calamity in your pocket. Handle lightly.";
  }
}