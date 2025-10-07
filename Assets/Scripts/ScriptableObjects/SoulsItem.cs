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
        int baseCost  = cost;
        int purchases = player.soulsBoughtThisRun;

        return (purchases == 0) ? 0 : baseCost * purchases;
    }
}