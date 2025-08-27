using UnityEngine;
using System.Collections.Generic;
using DeckboundDungeon.GamePhase;
using UnityEngine.UI;

public class MerchantUI : MonoBehaviour
{
    public Image key;
    public RoomsManager roomsManager;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject canvaMerchant;
    [SerializeField] private GameObject cardSlotPrefab;
    [SerializeField] private GameObject merchantItemViewPrefab;
    [SerializeField] private GameObject soulsPanel;
    private PlayerController player;
    private CardManager deck;
    private GameManager gm;

    void Awake()
    {
        player = FindFirstObjectByType<PlayerController>();
        deck = FindFirstObjectByType<CardManager>();
        gm = FindFirstObjectByType<GameManager>();
    }

    public void Show(List<MerchantItem> shopItems)
    {
        foreach (Transform t in contentParent)
            Destroy(t.gameObject);

        // 2) Instanciar uno por uno
        foreach (var item in shopItems)
        {
            GameObject go;
            if (item is CardItem cardItem)
            {
                // si es carta usamos el prefab de carta
                go = Instantiate(cardSlotPrefab, contentParent);
                var ui = go.GetComponent<CardUI>();
                ui.SetCardUI(cardItem.cardData);

                Destroy(go.GetComponent<CardDragDrop>());
                Destroy(go.GetComponent<CardSelector>());
                Destroy(go.GetComponent<CardHoverInHand>());
                var buyBtn = go.GetComponentInChildren<Button>();
                buyBtn.onClick.RemoveAllListeners();
                buyBtn.onClick.AddListener(() =>
                {
                    if (player.AmountGold >= item.cost)
                    {
                        player.SpendGold(item.cost);
                        // En tu CardItem debes tener algo como:
                        gm.AddCardToDeck(cardItem.cardData);
                        gm.ShowMessage($"¡Has comprado “{item.itemName}”!", 2f);
                        buyBtn.interactable = false; // desactivar
                    }
                    else
                    {
                        gm.ShowMessage("No tienes suficiente oro", 2f);
                    }
                });
            }
            else
            {
                // si no, es pocion/llave: usamos merchantItemPrefab
                go = Instantiate(merchantItemViewPrefab, contentParent);
                var ui = go.GetComponent<MerchantItemView>();
                ui.Setup(item, OnBuyClicked);
            }
            // asegurar escala 1:1
            //go.GetComponent<RectTransform>().localScale = Vector3.one;
        }

        // 3) Mostrar el panel
        gameObject.SetActive(true);
        soulsPanel.SetActive(false);
    }

    private void OnBuyClicked(MerchantItem item)
    {
        int finalCost = item.cost;

        if (item is SoulsItem soulsItem)
            finalCost = soulsItem.GetDynamicCost(player);

        if (player.AmountGold < finalCost)
        {
            gm.ShowMessage("No tienes suficiente oro", 2f);
            return;
        }
        player.SpendGold(finalCost);
        item.Apply(player, gm);
        gm.ShowMessage($"Compraste “{item.itemName}”", 2f);

        if (item.itemName.Contains("Llave"))
        {
            gm.hasKey = true;
            key.gameObject.SetActive(true);
        }
    }

    public void DeactiveKey()
    {
        key.gameObject.SetActive(false);
    }

    public void Close()
    {
        canvaMerchant.SetActive(false);
        soulsPanel.SetActive(true);
        if (!gm.hasKey)
            gm.PreparationPhase();
    }
}