using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

public class WallPlacer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Configuración")]
    public GameObject wallPrefab;        // Prefab del muro con NavMeshObstacle
    public int wallCost = 50;            // Coste en oro
    public Tilemap tilemap;

    private GameObject currentWall;      // Muro temporal durante arrastre
    private Camera mainCamera;
    public PlayerController playerController;


    public float gridSize = 1f; // Tamaño de la cuadrícula (ej: 1 unidad)


    void Awake()
    {
        mainCamera = Camera.main;
    }

    // 1. Al comenzar el arrastre
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (CanAffordWall())
        {
            currentWall = Instantiate(wallPrefab);
            UpdateWallPosition(eventData);
        }
        else
        {
            Debug.Log("No tienes suficiente oro");
        }
    }

    // 2. Durante el arrastre
    public void OnDrag(PointerEventData eventData)
    {
        if (currentWall == null) return;

        UpdateWallPosition(eventData);
        //CheckPlacementValidity();
        //UpdateVisualFeedback();
    }

    // 3. Al soltar el muro
    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentWall != null && CanAffordWall())
        {
            PlaceFinalWall();
        }
        Destroy(currentWall);
    }

    bool CanAffordWall()
    {
        return playerController.AmountGold >= wallCost;
    }

    void UpdateWallPosition(PointerEventData eventData)
    {
        /*
        // Convertir posición del ratón a coordenadas del mundo
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(eventData.position);
        mousePos.z = 0; // Para vista 2D

        // Alinear a la cuadrícula
        Vector3 gridPos = new Vector3(
            Mathf.Round(mousePos.x / gridSize) * gridSize,
            Mathf.Round(mousePos.y / gridSize) * gridSize,
            0
        );

        currentWall.transform.position = gridPos;*/

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(eventData.position);
        mouseWorldPos.z = 0;

        // 1. Convertir a celda de tilemap
        Vector3Int cellPos = tilemap.WorldToCell(mouseWorldPos);

        // 2. Convertir la celda a la posición del centro de la celda en el mundo
        Vector3 alignedWorldPos = tilemap.GetCellCenterWorld(cellPos);

        // 3. Colocar el muro en esa posición
        currentWall.transform.position = alignedWorldPos;
    }


    void PlaceFinalWall()
    {
        playerController.SpendGold(wallCost);
        Instantiate(wallPrefab, currentWall.transform.position, Quaternion.identity);
    }
}

