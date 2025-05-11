using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using DeckboundDungeon.Cards;

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
    public PlayerController player;

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
    }

    void Start()
    {
        highlightTile = Resources.Load<TileBase>("Tiles/HighlightTile");
        highlightMap = GameObject.Find("Tilemap Highlighted").GetComponent<Tilemap>();
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
        if (!player.TrySpendSouls(cardData.cost))
        {
            Debug.Log("No tienes suficientes almas para jugar " + cardData.cardName);
            // sonido de que la carta no se puede jugar
            eventData.pointerDrag       = null;
            eventData.pointerPress      = null;
            canvasGroup.blocksRaycasts  = true;
            isDragging                  = false;
            canvasGroup.alpha           = 1f;
            return;
        }
        originalAnchoredPosition = rectTransform.anchoredPosition;
        originalTransform = transform.parent;
        cardIndex = transform.GetSiblingIndex();
        transform.SetParent(layerDrag, true);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;

        canvasGroup.alpha = 0.3f;
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        rectTransform.position = Input.mousePosition;        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = dropTilemap.WorldToCell(worldPos);

        if (dropTilemap.HasTile(cellPos))
        {
            Collider2D col = Physics2D.OverlapPoint(new Vector2(worldPos.x, worldPos.y));
            if(col != null){
                SalaController sala = col.GetComponent<SalaController>();
                if(sala != null && sala.estaLibre){ 
                    if(cardData.cardType == CardType.Trap){
                        Color initialColor = dropTilemap.GetColor(cellPos);
                        dropTilemap.SetTileFlags(cellPos, TileFlags.None);
                        dropTilemap.SetColor(cellPos, Color.green);
                        var trapGO = Instantiate(((TrapCardData)cardData).trapPrefab, dropTilemap.GetCellCenterWorld(cellPos), Quaternion.identity);
                        var trapController = trapGO.GetComponent<TrapController>();
                        trapController.cardData = (TrapCardData)cardData;
                        trapController.player = player;
                        trapController.cellPos = cellPos;
                        trapController.initialColor = initialColor;
                    }
                    if(cardData.cardType == CardType.DeckEffect){
                        if(Deck.discardPile.Count == 0){
                            Debug.Log("No hay cartas en la pila de descartes");
                            goto End_Drop;
                        }
                    }

                    Vector3 worldCenter = dropTilemap.GetCellCenterWorld(cellPos);
                    cardData.Play(player, worldCenter);
                    if (hasPrevious)
                    {
                        highlightMap.SetTile(previousCell, null);
                        hasPrevious = false;
                    }
                    player.SpendSouls(cardData.cost);
                    Deck.CardPlayed(gameObject, cardData);
                    //Destroy(gameObject);
                    cartaColocada = true;
                }
            }
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            isDragging = false;
        }
        End_Drop:
        if(!cartaColocada)
        {
            transform.SetParent(originalTransform, true);
            transform.SetSiblingIndex(cardIndex);
            rectTransform.anchoredPosition = originalAnchoredPosition;
        }

        canvasGroup.blocksRaycasts = true;
    }

    /*private bool IsOverLayout()
    {
        if (raycasterUI == null)
        {
            Debug.LogError("El GraphicRaycaster no está asignado a raycasterUI.");
            return false;
        }
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        raycasterUI.Raycast(pointerData, results);

        foreach (var result in results)
        {
            Debug.Log("Encontrado componente: " + result.gameObject.name);
            if (result.gameObject.name == "PanelCartas")
                return true;
        }

        return false;
    }*/
 
}
