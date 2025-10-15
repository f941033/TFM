using UnityEngine;

public abstract class MerchantItem : ScriptableObject {
  public string itemName;
  public Sprite icon;
  public int cost;
  public abstract void Apply(PlayerController player, GameManager game);
  [TextArea] public string purchaseLine;

  public virtual string GetPurchaseLine(PlayerController p, GameManager g, int paidGold) {
    if (!string.IsNullOrEmpty(purchaseLine)) {
      return purchaseLine
        .Replace("{cost}", paidGold.ToString())
        .Replace("{item}", itemName);
    }

    return paidGold == 0
      ? $"“{itemName}” for free. How curious."
      : $"You bought “{itemName}” for {paidGold}.";
  }
}