using UnityEngine;
using System.Collections.Generic;
using DeckboundDungeon.GamePhase;
using UnityEngine.UI;
using System.Collections;

public class MerchantUI : MonoBehaviour
{
    public Image key;
    public GameObject panelKeyInfo;
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
            entry.Setup(item, player, OnBuyClicked);
        }

        gameObject.SetActive(true);
        soulsPanel.SetActive(false);
    }

    private void OnBuyClicked(MerchantItem item)
    {
        int finalCost = item is SoulsItem soulsItem
        ? soulsItem.GetDynamicCost(player)
        : item.cost;

        if (player.AmountGold < finalCost)
        {
            gm.ShowMessage("No gold? What a pitiful beggar of your own fate.", 2f);
            return;
        }

        player.SpendGold(finalCost);
        item.Apply(player, gm);

        gm.ShowMessage($"You bought “{item.itemName}” for {finalCost}", 2f);

        if (item.itemName.Contains("Key"))
        {
            gm.hasKey = true;
            key.gameObject.SetActive(true);
            panelKeyInfo.SetActive(true);
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
        else
        {
            gm.ShowMessage("Open a room. If you crave your doom.", 3f);
            StartCoroutine(gm.KeyVFX());
        }
    }

}