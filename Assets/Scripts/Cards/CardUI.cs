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
    [SerializeField] private TextMeshProUGUI goldCostText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button detailsButton;
    [SerializeField] private Sprite typeBackgroundTrap;
    [SerializeField] private Sprite cooldownSprite;
    [SerializeField] private Image cooldownFillImage;
    [SerializeField] private TextMeshProUGUI textType;

    public void SetCardUI(CardData cardData)
    {
        var sprite = Resources.Load<Sprite>("Sprites/Cards/" + cardData.name);
        data = cardData;
        textName.text = data.cardName;
        textDescription.text = data.description;
        goldCostText.text = data.goldCost.ToString();
        bool inShop = GameManager.CurrentPhase == GamePhase.Merchant;
        goldCostText.gameObject.SetActive(inShop);
        buyButton.gameObject.SetActive(inShop);
        detailsButton.gameObject.SetActive(GameManager.CurrentPhase == GamePhase.Deck);

        //Esto es para añadir el sprite de la carta
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
            if (trap.IsAreaDamage)
                damageImage.sprite = areaDamageImage;
        }
        else if (cardData is DeckEffectCardData deck)
        {
            textCost.gameObject.SetActive(false);
            backgroundImage.sprite = deckBackground;
            damageImage.gameObject.SetActive(false);
            costImage.gameObject.SetActive(false);
            textDamage.gameObject.SetActive(false);
            textType.text = "BOUND";
        }
        else
        {
            if (cardData is HabilityCardData hab)
            {
                if (!hab.IsDamage)
                {
                    damageImage.gameObject.SetActive(false);
                    textDamage.gameObject.SetActive(false);
                }
                if(hab.IsAreaDamage)
                    damageImage.sprite = areaDamageImage;
            }
            textCost.gameObject.SetActive(false);
            backgroundImage.sprite = buffBackground;
            damageImage.gameObject.SetActive(false);
            textDamage.gameObject.SetActive(false);
            costImage.sprite = cooldownSprite;
            cooldownFillImage.gameObject.SetActive(true);
            Debug.Log("Es una carta de buff o hab y activo el cooldownImage");
            textType.text = "SPELL";
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

    public void Initialize(GameManager gm, CardManager deck)
    {
        // Limpiamos listeners
        buyButton.onClick.RemoveAllListeners();

        // Añadimos el nuestro
        buyButton.onClick.AddListener(() =>
        {
            var data = this.data;
            gm.AddCardToDeck(data);
            gm.ShowMessage($"Carta «{data.cardName}» añadida al mazo", 2f);
            buyButton.interactable = false;
        });
    }
        public void ShowData()
    {
        Debug.Log("Estoy clicando la carta y voy a mostrar los datos");
        // Solo en fases donde quieras permitir ver detalles:
        if (GameManager.CurrentPhase == GamePhase.Deck ||
            GameManager.CurrentPhase == GamePhase.Pause)
        {
            var cardDetails = FindFirstObjectByType<CardDetailUI>();
             if (cardDetails == null)
            {
                Debug.LogWarning("[CardUI] No hay ningún CardDetailUI en escena");
                return;
            }
            cardDetails.Show(data);
        }
    }
}
