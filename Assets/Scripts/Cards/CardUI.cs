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
    [HideInInspector] public CardData data;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite trapBackground;
    [SerializeField] private Sprite buffBackground;
    [SerializeField] private Sprite deckBackground;
    [SerializeField] private Image damageImage;
    [SerializeField] private Sprite areaDamageImage;
    [SerializeField] private Image costImage;
    [SerializeField] private Image spriteImage;
    [SerializeField] private Button detailsButton;
    [SerializeField] private Sprite typeBackgroundTrap;
    [SerializeField] private Sprite cooldownSprite;
    [SerializeField] private Image cooldownFillImage;
    [SerializeField] private TextMeshProUGUI textType;
    [SerializeField] private Image usesImage;
    [SerializeField] private TextMeshProUGUI textUses;
    [SerializeField] private Sprite minionBackground;
    [SerializeField] private GameObject minionDescription;
    [SerializeField] private TextMeshProUGUI minionDamage;
    [SerializeField] private TextMeshProUGUI minionHealth;
    [SerializeField] private TextMeshProUGUI minionRange;
    [SerializeField] private Sprite minionEgg;


    public void SetCardUI(CardData cardData)
    {
        var sprite = Resources.Load<Sprite>("Sprites/Cards/" + cardData.name);
        data = cardData;
        textName.text = data.cardName;
        textDescription.text = data.description;
        bool inShop = GameManager.CurrentPhase == GamePhase.Merchant;
        detailsButton.gameObject.SetActive(GameManager.CurrentPhase == GamePhase.Deck);

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

        //Esto es para añadir el fondo y todos los componentes de la carta que no son el sprite
        if (cardData is TrapCardData trap)
        {
            textCost.text = trap.cost.ToString();
            backgroundImage.sprite = trapBackground;
            textDamage.text = trap.damage.ToString();
            textType.text = "TRAP";
            if (trap.damage == 0)
                damageImage.gameObject.SetActive(false);
            if (trap.IsAreaDamage)
                    damageImage.sprite = areaDamageImage;
            if (trap.uses > 0)
            {
                usesImage.gameObject.SetActive(true);
                textUses.text = trap.uses.ToString();
            }
        }
        else if (cardData is DeckEffectCardData deck)
        {
            backgroundImage.sprite = deckBackground;
            textCost.text = deck.cost.ToString();
            damageImage.gameObject.SetActive(false);
            textDamage.gameObject.SetActive(false);
            textType.text = "BOUND";
        }
        else if (cardData is SummonCardData summon)
        {
            backgroundImage.sprite = minionBackground;
            textCost.text = summon.cost.ToString();
            textDamage.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
            damageImage.sprite = minionEgg;
            minionDescription.SetActive(true);
            textDamage.text = "x" + summon.numberOfMinions;
            minionHealth.text = summon.minionHealth.ToString();
            minionDamage.text = summon.minionDamage.ToString();
            minionRange.text = summon.minionRange.ToString();
            textDescription.alignment = TextAlignmentOptions.Top;
            textType.text = "SUMMON";
        }
        else
        {
            if (cardData is HabilityCardData hab)
            {
                textCost.text = hab.cooldown.ToString();
                if (!hab.IsDamage)
                {
                    damageImage.gameObject.SetActive(false);
                    textDamage.gameObject.SetActive(false);
                }
                if (hab.IsAreaDamage)
                    damageImage.sprite = areaDamageImage;
            }
            if (cardData is BuffCardData buff)
                textCost.text = buff.cooldown.ToString();
            backgroundImage.sprite = buffBackground;
            damageImage.gameObject.SetActive(false);
            textDamage.gameObject.SetActive(false);
            costImage.sprite = cooldownSprite;
            if (ColorUtility.TryParseHtmlString("#00FFF7", out var c))
            costImage.color = c;
            cooldownFillImage.gameObject.SetActive(true);
            textType.text = "SPELL";
        }
        if (GameManager.CurrentPhase == GamePhase.Preparation ||
            GameManager.CurrentPhase == GamePhase.Action)
        {
            Destroy(this.GetComponent<CardSelector>());
        }
    }

    void Awake()
    {
        HandlePhaseChanged(GameManager.CurrentPhase);
    }

    void OnDisable()
    {
    }

    private void HandlePhaseChanged(GamePhase newPhase)
    {
        bool inShop = newPhase == GamePhase.Merchant;
    }
    public void ShowData()
    {
        if (GameManager.CurrentPhase == GamePhase.Deck ||
            GameManager.CurrentPhase == GamePhase.Pause)
        {
            var cardDetails = FindFirstObjectByType<CardDetailUI>();
             if (cardDetails == null) return;

            cardDetails.Show(data);
        }
    }
}
