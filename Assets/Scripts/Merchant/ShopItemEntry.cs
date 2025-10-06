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
    private GameManager gm;
    private System.Action<MerchantItem> onBuy;

    public void Setup(MerchantItem item, PlayerController playerRef, System.Action<MerchantItem> onBuyClicked)
    {
        if (!slotImage || !nameText || !costText || !buyButton)
        { Debug.LogError("[ShopItemEntry] Falta asignar refs en el inspector"); return; }

        data   = item;
        player = playerRef;
        gm = FindFirstObjectByType<GameManager>();
        onBuy  = onBuyClicked;

        // pinta UI hija (igual que ya tenías) ...
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
            if (view)
            {
                if (view.icon)
                {
                    view.icon.sprite = item.icon;
                    view.icon.preserveAspect = true;
                }
                if (view.nameText)  view.nameText.gameObject.SetActive(false);
                if (view.costText)  view.costText.gameObject.SetActive(false);
                if (view.buyButton) view.buyButton.gameObject.SetActive(false);
            }
        }

        // coste visible (FREE o número)
        costText.text = GetCostLabel(data);

        // click
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnContainerBuyClicked);

        // estado inicial del botón (incluye oro suficiente)
        UpdateBuyButtonInteractable();

        // suscríbete para actualizar si cambia el oro
        // (el evento pasa el delta, pero usamos player.AmountGold para comparar)
        player.OnGoldChanged += HandleGoldChanged;
    }

    private void HandleGoldChanged(int _)
    {
        UpdateBuyButtonInteractable();
    }

    private void UpdateBuyButtonInteractable()
    {
        bool isKey = data is KeyItem || (data.itemName != null && data.itemName.Contains("Llave"));
        bool blockKeyNoRooms= isKey && gm != null && gm.closedRooms == 0;
        bool blockKeyHasOne = isKey && gm != null && gm.hasKey;

        bool blockSoulsCap  = data is SoulsItem && player.soulsBuyPerShop >= 3;

        int finalCost = GetNumericCost(data);
        bool hasGold = player.AmountGold >= finalCost;
        bool canBuy = !blockKeyNoRooms && !blockKeyHasOne && !blockSoulsCap && hasGold;

        buyButton.interactable = canBuy;
    }
    private void OnContainerBuyClicked()
    {
        int beforeGold = player.AmountGold;

        onBuy?.Invoke(data);

        costText.text = GetCostLabel(data);

        //if (player.AmountGold >= beforeGold) return;

        if (data is SoulsItem s)
        {
            if (player.soulsBuyPerShop >= 3)
            
                buyButton.interactable = false;
        }else if (data is KeyItem)
        {
            buyButton.interactable = false;
        }
        else
        {
            buyButton.interactable = false;
        }
    }
    
    private int GetNumericCost(MerchantItem item)
    {
        if (item is SoulsItem s) return s.GetDynamicCost(player);
        if (item is KeyItem   k) return k.GetDynamicCost(gm);
        return item.cost;
    }

    private string GetCostLabel(MerchantItem item)
    {
        int c = GetNumericCost(item);
        return c == 0 ? "FREE" : c.ToString();
    }
}
