using UnityEngine;
using UnityEngine.UI;

public class ShopItemEntry : MonoBehaviour
{
    [SerializeField] private GameObject cardSlotPrefab;
    [SerializeField] private GameObject merchantItemViewPrefab;

    public void Setup(MerchantItem item, System.Action<MerchantItem> onBuyClicked)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        if (item is CardItem cardItem)
        {
            var go = Instantiate(cardSlotPrefab, transform);
            go.GetComponent<CardUI>().SetCardUI(cardItem.cardData);

            Destroy(go.GetComponent<CardDragDrop>());
            Destroy(go.GetComponent<CardSelector>());
            Destroy(go.GetComponent<CardHoverInHand>());

            var buyBtn = go.GetComponentInChildren<Button>();
            buyBtn.onClick.RemoveAllListeners();
            buyBtn.onClick.AddListener(() =>
            {
                onBuyClicked(item); 
                buyBtn.interactable = false;
            });
        }

        else
        {
            var go = Instantiate(merchantItemViewPrefab, transform);
            go.GetComponent<MerchantItemView>().Setup(item, onBuyClicked);
        }
    }
}
