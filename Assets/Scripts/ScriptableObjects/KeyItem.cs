using UnityEngine;
[CreateAssetMenu(menuName="Shop/Key Item")]
public class KeyItem : MerchantItem {
  public override void Apply(PlayerController player, GameManager game) {
    player.AddKey();
  }
}