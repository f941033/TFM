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
    }

    private void OnBuyClicked(MerchantItem item)
    {
        if (item is PotionItem && player.CurrentHealth >= player.BaseHealth)
        {
            gm.ShowMessage("¡Tu vida ya está completa, no seas un gastizo!", 2f);
            return;
        }
        if (player.AmountGold < item.cost)
        {
            gm.ShowMessage("No tienes suficiente oro", 2f);
            return;
        }
        player.SpendGold(item.cost);
        item.Apply(player, gm);
        gm.ShowMessage($"Compraste “{item.itemName}”", 2f);
    }

    public void Close()
    {
        canvaMerchant.SetActive(false);
        gm.PreparationPhase();
    }
}