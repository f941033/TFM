using UnityEngine;
using System.Collections.Generic;

public class MerchantUI : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject merchantItemViewPrefab; // el prefab de arriba

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
        contentParent.gameObject.SetActive(true);
        foreach (Transform t in contentParent) Destroy(t.gameObject);
        foreach (var item in shopItems)
        {
            var go = Instantiate(merchantItemViewPrefab, contentParent);
            var view = go.GetComponent<MerchantItemView>();
            view.Setup(item, OnBuyClicked);
        }
    }

    private void OnBuyClicked(MerchantItem item)
    {
        if (player.AmountGold < item.cost)
        {
            gm.ShowMessage("No tienes suficiente oro", 2f);
            return;
        }
        player.SpendGold(item.cost);
        item.Apply(player, gm);
        gm.ShowMessage($"Compraste “{item.itemName}”", 2f);
        // opcional: desactivar el botón tras comprar
    }

    public void Close()
    {
        contentParent.gameObject.SetActive(false);
        gm.PreparationPhase();
    }
}