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

    // NUEVO: estado
    private bool purchased = false;
    private bool isKey = false;

    void OnDisable()
    {
        if (player != null) player.OnGoldChanged -= HandleGoldChanged;
    }

    void OnDestroy()
    {
        if (player != null) player.OnGoldChanged -= HandleGoldChanged;
    }

    public void Setup(MerchantItem item, PlayerController playerRef, System.Action<MerchantItem> onBuyClicked)
    {
        if (!slotImage || !nameText || !costText || !buyButton)
        { Debug.LogError("[ShopItemEntry] Falta asignar refs en el inspector"); return; }

        data   = item;
        player = playerRef;
        gm     = FindFirstObjectByType<GameManager>();
        onBuy  = onBuyClicked;

        purchased = false;
        isKey = (item is KeyItem) || (item.itemName != null && item.itemName.Contains("Llave"));

        // Limpieza del “slot”
        var slot = slotImage.transform;
        for (int i = slot.childCount - 1; i >= 0; i--) Destroy(slot.GetChild(i).gameObject);

        // Contenido visual (igual que tenías)
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
                if (view.icon) { view.icon.sprite = item.icon; view.icon.preserveAspect = true; }
                if (view.nameText)  view.nameText.gameObject.SetActive(false);
                if (view.costText)  view.costText.gameObject.SetActive(false);
                if (view.buyButton) view.buyButton.gameObject.SetActive(false);
            }
        }

        // Coste inicial (FREE o número)
        costText.text = GetCostLabel(data);

        // Click
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnContainerBuyClicked);

        // Estado inicial del botón
        UpdateBuyButtonInteractable();

        // Suscripción a cambios de oro
        player.OnGoldChanged += HandleGoldChanged;
    }

    private void HandleGoldChanged(int _)
    {
        UpdateBuyButtonInteractable();
    }

    private void UpdateBuyButtonInteractable()
    {
        // Guardas contra destrucción
        if (this == null) return;
        if (buyButton == null) return; // <- previene MissingReferenceException

        // No reactivar si ya se compró (cartas one-shot y llave si la tratas como one-shot)
        if (purchased && !(data is SoulsItem))
        {
            buyButton.interactable = false;
            return;
        }

        // Reglas de llave
        bool blockKeyNoRooms = isKey && gm != null && gm.closedRooms == 0;
        bool blockKeyHasOne  = isKey && gm != null && gm.hasKey;

        // Límite 3 almas por tienda
        bool blockSoulsCap   = data is SoulsItem && player.soulsBuyPerShop >= 3;

        // Oro suficiente (FREE = 0 permite comprar)
        int finalCost = GetNumericCost(data);
        bool hasGold  = player.AmountGold >= finalCost;

        bool canBuy = !blockKeyNoRooms && !blockKeyHasOne && !blockSoulsCap && hasGold;
        buyButton.interactable = canBuy;
    }

    private void OnContainerBuyClicked()
    {
        // Ejecuta compra (cobra + Apply)
        onBuy?.Invoke(data);

        // Refresca coste (FREE o número) y estado del botón
        costText.text = GetCostLabel(data);

        // Si es carta normal: una sola vez
        if (!(data is SoulsItem) && !isKey)
        {
            purchased = true;
        }

        // Si es llave y la quieres one-shot
        if (isKey)
        {
            purchased = true; // o confía en gm.hasKey para bloquear
        }

        UpdateBuyButtonInteractable();
    }

    // --- Helpers de coste ---
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
