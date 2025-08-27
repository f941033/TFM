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
        PlayerController player = FindFirstObjectByType<PlayerController>();
        data = item;
        icon.sprite = item.icon;
        nameText.text = item.itemName;

        if (item is SoulsItem soulsItem)
            costText.text = soulsItem.GetDynamicCost(player).ToString();
        else
            costText.text = item.cost.ToString();
        onBuy = onBuyClicked;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() =>
        {
            onBuy(data);
            if (item is SoulsItem)
            {
                if (player.soulsBuyPerShop >= 3)
                    buyButton.interactable = false;
                else
                    costText.text = (item as SoulsItem).GetDynamicCost(player).ToString();
            }
            else
            {
                // para el resto de ítems solo se compra una vez
                buyButton.interactable = false;
            }
        });
    }
}