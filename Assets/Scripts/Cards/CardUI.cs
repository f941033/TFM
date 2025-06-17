using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DeckboundDungeon.GamePhase;
public class CardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textName;
    [SerializeField] private TextMeshProUGUI textDescription;
    [SerializeField] private TextMeshProUGUI textCost;
    [SerializeField] private TextMeshProUGUI textDamage;
    [SerializeField] private TextMeshProUGUI textUse;
    [HideInInspector] public CardData data;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite trapBackground;
    [SerializeField] private Sprite buffBackground;
    [SerializeField] private Image damageImage;
    [SerializeField] private Image useImage;
    [SerializeField] private Image costImage;
    [SerializeField] private Image spriteImage;
    [SerializeField] private TextMeshProUGUI goldCostText;

    public void setCardUI(CardData cardData)
    {
        var sprite = Resources.Load<Sprite>("Sprites/Cards/" + cardData.name);
        data = cardData;
        textName.text = data.cardName;
        textDescription.text = data.description;
        goldCostText.text = data.goldCost.ToString();
        bool inShop = GameManager.CurrentPhase == GamePhase.Merchant;
        Debug.Log("Esta carta" + cardData.name + " va a mostrar el oro? " + inShop);
        goldCostText.gameObject.SetActive(inShop);
        if (sprite != null)
        {
            spriteImage.sprite = sprite;
            spriteImage.enabled = true;
        }
        else
        {
            spriteImage.enabled = false;
            Debug.LogWarning($"[CardUI] No encontré el sprite Resources/Sprites/Cards/{cardData.name}.png");
        }

        if (cardData is TrapCardData trap)
        {
            textCost.text = trap.cost.ToString();
            textUse.text = trap.uses.ToString();
            backgroundImage.sprite = trapBackground;
        }
        else
        {
            textCost.gameObject.SetActive(false);
            textUse.gameObject.SetActive(false);
            backgroundImage.sprite = buffBackground;
            damageImage.gameObject.SetActive(false);
            useImage.gameObject.SetActive(false);
            costImage.gameObject.SetActive(false);
        }
        if (data.cardType == CardType.Trap)
        {
            var trapData = cardData as TrapCardData;
            if (trapData != null)
                textDamage.text = trapData.damage.ToString();
        }
        else
        {
            textDamage.gameObject.SetActive(false);
        }
    }

    void Awake()
    {
        //GameManager gameManager = FindFirstObjectByType<GameManager>();
        //GameManager.OnPhaseChanged += HandlePhaseChanged;
        // Ajustamos el estado inicial **con la fase actual**
        HandlePhaseChanged(GameManager.CurrentPhase);
        Debug.Log(GameManager.CurrentPhase);
    }

    void OnDisable()
    {
    }

    private void HandlePhaseChanged(GamePhase newPhase)
    {
        // Solo mostrar el texto de coste de oro en la fase de tienda
        bool inShop = newPhase == GamePhase.Merchant;
        goldCostText.gameObject.SetActive(inShop);
    }
}
