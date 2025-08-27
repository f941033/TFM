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
        return cost + (player.soulsBuyPerShop * 50);
    }
}