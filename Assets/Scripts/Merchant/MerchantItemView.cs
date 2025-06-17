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
        data = item;
        icon.sprite = item.icon;
        nameText.text = item.itemName;
        costText.text = item.cost.ToString();
        onBuy = onBuyClicked;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() =>
        {
            onBuy(data);
            buyButton.interactable = false;
        });
    }
}