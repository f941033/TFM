using UnityEngine;
[CreateAssetMenu(menuName = "Shop/Souls")]
public class SoulsItem : MerchantItem
{
    public override void Apply(PlayerController player, GameManager game)
    {
        player.AddSouls();
    }
    public int GetDynamicCost(PlayerController player)
    {
        int baseCost = cost;
        int purchases = player.soulsBoughtThisRun;

        return (purchases == 0) ? 0 : baseCost * purchases;
    }
    public override string GetPurchaseLine(PlayerController p, GameManager g, int paidGold) {
        if (!string.IsNullOrEmpty(purchaseLine))
            return base.GetPurchaseLine(p, g, paidGold);
        return paidGold == 0 ? "A free sip of power." : "Power, bottled... try not to spill it.";
    }
}