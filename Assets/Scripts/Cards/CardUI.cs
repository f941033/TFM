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
    [SerializeField] private Image damageImage;
    [SerializeField] private Image costImage;
    [SerializeField] private Image spriteImage;
    [SerializeField] private TextMeshProUGUI goldCostText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button detailsButton;
    [SerializeField] private Image descriptionImage;
    [SerializeField] private Sprite descriptionBackgroundTrap;
    [SerializeField] private Image typeImage;
    [SerializeField] private Sprite typeBackgroundTrap;
    [SerializeField] private TextMeshProUGUI textType;
    [SerializeField] private Image vortexImage;
    [SerializeField] private Sprite vortexBackgroundTrap;

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
            descriptionImage.sprite = descriptionBackgroundTrap;
            typeImage.sprite = typeBackgroundTrap;
            textType.text = "TRAP";
            vortexImage.sprite = vortexBackgroundTrap;
        }
        else
        {
            textCost.gameObject.SetActive(false);
            backgroundImage.sprite = buffBackground;
            damageImage.gameObject.SetActive(false);
            costImage.gameObject.SetActive(false);
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
