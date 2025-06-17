using UnityEngine;
using System.Collections.Generic;
using DeckboundDungeon.GamePhase;
using UnityEngine.UI;

public class MerchantUI : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject canvaMerchant;
    [SerializeField] private GameObject cardSlotPrefab;
    [SerializeField] private GameObject merchantItemViewPrefab;
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
        foreach (var item in shopItems) {
        GameObject go;
            if (item is CardItem cardItem)
            {
                // si es carta usamos el prefab de carta
                go = Instantiate(cardSlotPrefab, contentParent);
                var ui = go.GetComponent<CardUI>();
                ui.setCardUI(cardItem.cardData);
                /* var btn = go.GetComponentInChildren<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnBuyClicked(cardItem)); */
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
        canvaMerchant.SetActive(false);
        Debug.Log("Clico en cerrar");
        //gm.PreparationPhase();
    }
}