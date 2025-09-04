using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemEntry : MonoBehaviour
{
    [SerializeField] private Image slotImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button buyButton;
    [SerializeField] private GameObject cardSlotPrefab;
    [SerializeField] private GameObject merchantItemViewPrefab;

    private MerchantItem data;
    private PlayerController player;
    private System.Action<MerchantItem> onBuy;

    public void Setup(MerchantItem item, PlayerController playerRef, System.Action<MerchantItem> onBuyClicked)
    {
        if (!slotImage) { Debug.LogError("[ShopItemEntry] slotImage no asignado."); return; }
        if (!nameText || !costText || !buyButton) { Debug.LogError("[ShopItemEntry] Asigna nameText, costText y buyButton en el contenedor."); return; }

        data   = item;
        player = playerRef;
        onBuy  = onBuyClicked;

        costText.text = GetDisplayCost(item).ToString();

        var slot = slotImage.transform;
        for (int i = slot.childCount - 1; i >= 0; i--) Destroy(slot.GetChild(i).gameObject);

        if (item is CardItem cardItem)
        {
            var go = Instantiate(cardSlotPrefab, slot, false);
            var ui = go.GetComponent<CardUI>();
            ui.SetCardUI(cardItem.cardData);
            nameText.gameObject.SetActive(false);

            var innerBtn = go.GetComponentInChildren<Button>();
            if (innerBtn) innerBtn.gameObject.SetActive(false);
            Destroy(go.GetComponent<CardDragDrop>());
            Destroy(go.GetComponent<CardSelector>());
            Destroy(go.GetComponent<CardHoverInHand>());
        }
        else
        {
            nameText.text = item.itemName;
            var go = Instantiate(merchantItemViewPrefab, slot, false);
            var view = go.GetComponent<MerchantItemView>();
            view.icon.preserveAspect = true;
            if (view)
            {
                if (view.icon) view.icon.sprite = item.icon;
                if (view.nameText) view.nameText.gameObject.SetActive(false);
                if (view.costText) view.costText.gameObject.SetActive(false);
                if (view.buyButton) view.buyButton.gameObject.SetActive(false);
            }
        }

        var gm = FindFirstObjectByType<GameManager>();

        bool isKey = item is KeyItem || (item.itemName != null && item.itemName.Contains("Llave"));

        bool blockKey = isKey && gm != null && gm.closedRooms == 0;
        bool blockSouls = item is SoulsItem && player.soulsBuyPerShop >= 3;
        bool canBuy = !blockKey && !blockSouls;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnContainerBuyClicked);

        buyButton.interactable = canBuy;
    }

    private int GetDisplayCost(MerchantItem item)
        => item is SoulsItem s ? s.GetDynamicCost(player) : item.cost;

    private void OnContainerBuyClicked()
    {
        int beforeGold = player.AmountGold;

        onBuy?.Invoke(data);

        if (player.AmountGold >= beforeGold) return;

        if (data is SoulsItem s)
        {
            if (player.soulsBuyPerShop >= 3)
                buyButton.interactable = false;
            else
                costText.text = s.GetDynamicCost(player).ToString();
        }
        else
        {
            buyButton.interactable = false;
        }
    }
}
