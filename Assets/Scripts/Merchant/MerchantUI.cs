using UnityEngine;
using System.Collections.Generic;
using DeckboundDungeon.GamePhase;
using UnityEngine.UI;
using System.Collections;
using TMPro;

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
    [SerializeField] private TextMeshProUGUI speechBubbleText;
    private Coroutine speechCO;

    public void Speak(string text, float duration = 2f)
    {
        if (!speechBubbleText) return;
        if (speechCO != null) StopCoroutine(speechCO);

        speechBubbleText.text = text;
        speechCO = StartCoroutine(HideSpeechAfter(duration));
    }

    private IEnumerator HideSpeechAfter(float t)
    {
        yield return new WaitForSeconds(t);
        speechBubbleText.text = "The deal is on the table. \n Buy what you will, if your wretched coins allow it.";
        speechCO = null;
    }

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
        int finalCost = item.cost;
        if (item is SoulsItem s) finalCost = s.GetDynamicCost(player);
        else if (item is KeyItem k) finalCost = k.GetDynamicCost(gm);

        if (player.AmountGold < finalCost)
        {
            gm.ShowMessage("No gold? What a pitiful beggar of your own fate.", 2f);
            return;
        }

        player.SpendGold(finalCost);
        item.Apply(player, gm);

        var line = item.GetPurchaseLine(player, gm, finalCost);
        Speak(line, 3f);

        if (item is KeyItem)
        {
            gm.hasKey = true;
            gm.keysBoughtThisRun++;
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