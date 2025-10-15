using UnityEngine;
[CreateAssetMenu(menuName = "Shop/Key Item")]
public class KeyItem : MerchantItem
{
  public override void Apply(PlayerController player, GameManager game)
  {
    player.AddKey();
  }

  public int GetDynamicCost(GameManager gm)
  {
    int baseCost = cost;
    int purchases = gm.keysBoughtThisRun;
    return (purchases == 0) ? 0 : baseCost * purchases;
  }
  public override string GetPurchaseLine(PlayerController p, GameManager g, int paidGold) {
    if (!string.IsNullOrEmpty(purchaseLine))
      return base.GetPurchaseLine(p, g, paidGold);
    return paidGold == 0 ? "A key, freely given... ominous." : "Take the key. See what it unlocks... if you dare.";
  }
}