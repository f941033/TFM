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
      int baseCost  = cost;
      int purchases = gm.keysBoughtThisRun;
      return (purchases == 0) ? 0 : baseCost * purchases;
  }
}