using UnityEngine;
[CreateAssetMenu(menuName="Shop/Potion Item")]
public class PotionItem : MerchantItem {
  [Tooltip("Porcentaje de salud máxima a curar (0–1)")]
  public float healPercent = 0.6f;
  public override void Apply(PlayerController player, GameManager game) {
    float amount = player.BaseHealth * healPercent;
    player.Heal(amount);
  }
}