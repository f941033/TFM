using UnityEngine;

public abstract class MerchantItem : ScriptableObject {
  public string itemName;
  public Sprite icon;
  public int cost;
  public abstract void Apply(PlayerController player, GameManager game);
}