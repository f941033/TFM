using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MerchantItemView : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nameText, costText;
    public Button buyButton;
    private MerchantItem data;
    private System.Action<MerchantItem> onBuy;

    public void Setup(MerchantItem item, System.Action<MerchantItem> onBuyClicked)
    {
        var player = FindFirstObjectByType<PlayerController>();
        var gm     = FindFirstObjectByType<GameManager>();

        data = item;
        icon.sprite = item.icon;
        icon.preserveAspect = true;
        nameText.text = item.itemName;

        // --- coste inicial ---
        costText.text = GetCostLabel(item, player, gm);

        onBuy = onBuyClicked;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() =>
        {
            onBuy(data);

            // Límite por tienda (almas)
            if (data is SoulsItem && player.soulsBuyPerShop >= 3)
                buyButton.interactable = false;

            // Refresca SIEMPRE el coste mostrado (FREE o número)
            costText.text = GetCostLabel(data, player, gm);

            // Si otros ítems no son recomprables, puedes desactivar:
            // if (!(data is SoulsItem || data is KeyItem)) buyButton.interactable = false;
        });
    }

    private string GetCostLabel(MerchantItem item, PlayerController player, GameManager gm)
    {
        if (item is SoulsItem s)
        {
            int c = s.GetDynamicCost(player);
            return c == 0 ? "FREE" : c.ToString();
        }
        if (item is KeyItem k)
        {
            int c = k.GetDynamicCost(gm);
            return c == 0 ? "FREE" : c.ToString();
        }
        return item.cost.ToString();
    }
}