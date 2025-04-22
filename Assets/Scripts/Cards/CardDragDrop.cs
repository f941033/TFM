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

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        layerDrag = GameObject.Find("LayerDrag")?.transform;
        if (layerDrag == null)
            Debug.LogError("No se encontró el DragLayer en la escena.");
        }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalAnchoredPosition = rectTransform.anchoredPosition;
        originalTransform = transform.parent;
        cardIndex = transform.GetSiblingIndex();
        transform.SetParent(layerDrag, true);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = dropTilemap.WorldToCell(worldPos);

        if (dropTilemap.HasTile(cellPos))
        {
            /*if(IsOverLayout()){
                Debug.Log("La has soltado sobre el layout y no sobre el mapa");
                return;
            }*/
            Collider2D col = Physics2D.OverlapPoint(new Vector2(worldPos.x, worldPos.y));
            if(col != null){
                SalaController sala = col.GetComponent<SalaController>();
                if(sala != null && sala.estaLibre){ 
                    if(cardData.cardType == CardType.Trap){
                        dropTilemap.SetTileFlags(cellPos, TileFlags.None);
                        dropTilemap.SetColor(cellPos, Color.green);
                    }
                    if(cardData.cardType == CardType.DeckEffect){
                        if(Deck.discardPile.Count == 0){
                            Debug.Log("No hay cartas en la pila de descartes");
                            goto End_Drop;
                        }
                    }
                    cardData.Play(player, cellPos);
                    Deck.CardPlayed(gameObject, cardData);
                    //Destroy(gameObject);
                    cartaColocada = true;
                }
            }
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
