using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using DeckboundDungeon.Cards;
using TMPro;

public class CardDragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalTransform;
    private Transform layerDrag;
    private int cardIndex;
    private Vector2 originalAnchoredPosition;
    public CardData cardData;
    internal CardManager Deck;
    internal GameManager gameManager;
    public PlayerController player;

    public Tilemap tilemap;
    public LayerMask obstacleLayers; // Capas de obstáculos (paredes, etc.)
    public LayerMask spawnPointLayers;
    public Tilemap obstacleTilemap; // Tilemap de obstáculos (opcional)
    public float checkRadius = 0.5f; // Radio para verificación de colisión
    public GameObject borderPrefab_1, borderPrefab_4;
    private GameObject currentBorder;        //borde temporal durante el arrastre


    //private TextMeshProUGUI textNumberOfCardsDiscard;
    //private GameObject discardPileImage;

    private bool cartaColocada = false;
    [SerializeField] private GraphicRaycaster raycasterUI;

    [Header("Zona válida")]
    public Tilemap dropTilemap;

    //Tilemap Higlighted
    public Tilemap highlightMap;
    public TileBase highlightTile;

    private Vector3Int previousCell;
    private bool hasPrevious = false;
    private bool isDragging = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        layerDrag = GameObject.Find("LayerDrag")?.transform;
        if (layerDrag == null)
            Debug.LogError("No se encontró el DragLayer en la escena.");
        player = FindFirstObjectByType<PlayerController>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    void Start()
    {
        highlightTile = Resources.Load<TileBase>("Tiles/HighlightTile");
        highlightMap = GameObject.Find("Tilemap Highlighted").GetComponent<Tilemap>();
        tilemap = GameObject.Find("Tilemap Laberinto").GetComponent<Tilemap>();
        obstacleTilemap = GameObject.Find("Paredes").GetComponent<Tilemap>();
    }

    void Update()
    {
        // Solo resalta si estás arrastrando una carta
        if (isDragging)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = highlightMap.WorldToCell(mouseWorldPos);

            // Solo actualiza si se ha cambiado de celda
            if (!hasPrevious || cellPos != previousCell)
            {
                if (hasPrevious)
                    highlightMap.SetTile(previousCell, null);

                highlightMap.SetTile(cellPos, highlightTile);
                previousCell = cellPos;
                hasPrevious = true;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (cardData is TrapCardData trap)
        {
            if (!player.TrySpendSouls(trap.cost))
            {
                FindFirstObjectByType<GameManager>().ShowMessage("¡No tienes suficientes almas!", 2);
                Debug.Log("No tienes suficientes almas para jugar " + cardData.cardName);
                // sonido de que la carta no se puede jugar
                eventData.pointerDrag = null;
                eventData.pointerPress = null;
                canvasGroup.blocksRaycasts = true;
                isDragging = false;
                canvasGroup.alpha = 1f;
                return;
            }
        }
        originalAnchoredPosition = rectTransform.anchoredPosition;
        originalTransform = transform.parent;
        cardIndex = transform.GetSiblingIndex();
        transform.SetParent(layerDrag, true);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;

        canvasGroup.alpha = 0.15f;
        isDragging = true;


        // poner en el cardData un atributo más para saber su escala
        // poner en el cardData un atributo más para saber su escala
        if (cardData.numberOfTiles == 4)
        {
            currentBorder = Instantiate(borderPrefab_4);
        }
        else
        {
            currentBorder = Instantiate(borderPrefab_1);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        rectTransform.position = Input.mousePosition;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        mouseWorldPos.z = 0;

        //------------------ILUMINAR CELDA AL DESPLAZAR-----------------

        Vector3Int cellPos = tilemap.WorldToCell(mouseWorldPos);

        if (IsPositionValid(cellPos))
        {
            Vector3 alignedPos = tilemap.GetCellCenterWorld(cellPos);
            currentBorder.transform.position = alignedPos;
            currentBorder.SetActive(true);
        }
        else
        {
            currentBorder.SetActive(false);
        }
    }

    bool IsPositionValid(Vector3Int cellPos)
    {
        Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos);

        // 1. Verificar tiles y colliders físicos
        bool isPhysicallyValid =
            tilemap.HasTile(cellPos) &&
            !Physics2D.OverlapCircle(worldPos, checkRadius, obstacleLayers) &&
            !Physics2D.OverlapCircle(worldPos, checkRadius, spawnPointLayers);

        // 2. Verificar distancia a Player
        bool isOutsidePlayerRange = true;
        float distance = Vector3.Distance(worldPos, player.transform.position);
        if (distance <= 2.25f)
        {
            isOutsidePlayerRange = false;
        }


        return isPhysicallyValid && isOutsidePlayerRange;
    }

    // Obtener todas las torretas en escena
    //TorretaController[] GetAllTurrets()
    //{
    //    return FindObjectsByType<TorretaController>(FindObjectsSortMode.None);
    //}

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Destroy(currentBorder);

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector3Int cellPos = dropTilemap.WorldToCell(worldPos);

        if (dropTilemap.HasTile(cellPos))
        {
            Collider2D col = Physics2D.OverlapPoint(new Vector2(worldPos.x, worldPos.y));
            if (col != null)
            {
                SalaController sala = col.GetComponent<SalaController>();
                if (sala != null && sala.estaLibre)
                {
                    if (cardData.cardType == CardType.Trap)
                    {
                        dropTilemap.SetTileFlags(cellPos, TileFlags.None);
                        //dropTilemap.SetColor(cellPos, Color.green);

                        Vector3 worldCenter = dropTilemap.GetCellCenterWorld(cellPos);
                        cardData.Play(player, worldCenter);

                    }
                    if (cardData.cardType == CardType.DeckEffect)
                    {
                        if (Deck.discardPile.Count == 0)
                        {
                            Debug.Log("No hay cartas en la pila de descartes");
                            goto End_Drop;
                        }
                    }
                    if (hasPrevious)
                    {
                        highlightMap.SetTile(previousCell, null);
                        hasPrevious = false;
                    }
                    Deck.CardPlayed(gameObject, cardData);
                    cartaColocada = true;
                }
            }
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            isDragging = false;
        }

    End_Drop:
        if (!cartaColocada)
        {
            transform.SetParent(originalTransform, true);
            transform.SetSiblingIndex(cardIndex);
            rectTransform.anchoredPosition = originalAnchoredPosition;
        }
        else
        {
            if (Deck.discardPile.Count == 1) gameManager.ActivateDiscardPileImage();

            if (cardData is TrapCardData trap)
                player.SpendSouls(trap.cost);
            gameManager.UpdateTextNumberOfCardsDiscard();
        }
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }
}
