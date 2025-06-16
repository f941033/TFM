using UnityEngine;
using System.Collections.Generic;

public class MerchantUI : MonoBehaviour {
  [SerializeField] private Transform contentParent;
  [SerializeField] private GameObject merchantItemPrefab;
  private List<MerchantItem> items;

  private PlayerController player;
  private CardManager deck;
  private GameManager gameManager;

  void Awake() {
    player = FindFirstObjectByType<PlayerController>();
    deck = FindFirstObjectByType<CardManager>();
    gameManager = FindFirstObjectByType<GameManager>();
  }

  /// <summary> Abre la ventana con esta lista de artículos </summary>
  public void Show(List<MerchantItem> shopItems) {
    items = shopItems;
    gameObject.SetActive(true);
    Populate();
  }

  private void Populate() {
    // Limpia
    foreach (Transform c in contentParent) Destroy(c.gameObject);

    // Instancia cada MerchantItem
    foreach (var it in items) {
      var go = Instantiate(merchantItemPrefab, contentParent);
      var view = go.GetComponent<MerchantItemView>();
      view.Setup(it, OnBuyClicked);
    }
  }

  void OnBuyClicked(MerchantItem item) {
    if (player.AmountGold < item.cost) {
      gameManager.ShowMessage("No tienes suficiente oro", 2f);
      return;
    }
    player.SpendGold(item.cost);
    item.Apply(player, gameManager);
    gameManager.ShowMessage($"Has comprado {item.itemName}", 2f);
    // opcional: deshabilitar botón o remover artículo
  }

  public void Close() {
    gameObject.SetActive(false);
    gameManager.PreparationPhase();
  }
}