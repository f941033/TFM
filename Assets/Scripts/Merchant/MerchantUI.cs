using UnityEngine;
using System.Collections.Generic;
using DeckboundDungeon.GamePhase;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class MerchantUI : MonoBehaviour
{
    public Image key;
    public RoomsManager roomsManager;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject canvaMerchant;
    [SerializeField] private GameObject cardSlotPrefab;
    [SerializeField] private GameObject merchantItemViewPrefab;
    [SerializeField] private GameObject shopItemEntryPrefab;
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

        foreach (var item in shopItems)
        {
            var entryGO = Instantiate(shopItemEntryPrefab, contentParent);
            var entry = entryGO.GetComponent<ShopItemEntry>();
            entry.Setup(item, OnBuyClicked);
        }

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

        if (item is CardItem cardItem)
            gm.AddCardToDeck(cardItem.cardData);

        gm.ShowMessage($"Compraste “{item.itemName}” por {finalCost}", 2f);

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
        else gm.ShowMessage("Selecciona la sala a abrir, si te atreves", 2f);
    }
}