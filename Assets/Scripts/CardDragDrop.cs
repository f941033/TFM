using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class CardDragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalTransform;
    private Transform layerDrag;
    private int cardIndex;
    private Vector2 originalAnchoredPosition;

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
        Debug.Log("Input.mousePosition: " + Input.mousePosition);
        Debug.Log("RectTransform.position antes: " + rectTransform.position);
        originalAnchoredPosition = rectTransform.anchoredPosition;
        originalTransform = transform.parent;
        cardIndex = transform.GetSiblingIndex();
        transform.SetParent(layerDrag, true);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        /*Vector2 localPoint;

        RectTransform parentRect = transform.parent as RectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, null, out localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }*/
        rectTransform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("Estoy soltando la carta y acabando el drag");
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = dropTilemap.WorldToCell(worldPos);

        if (dropTilemap.HasTile(cellPos))
        {
            Debug.Log("Carta soltada sobre un tile válido en: " + cellPos);
            Destroy(gameObject);
        }
        else
        {
            transform.SetParent(originalTransform, true);
            transform.SetSiblingIndex(cardIndex);
            rectTransform.anchoredPosition = originalAnchoredPosition;
        }

        canvasGroup.blocksRaycasts = true;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
