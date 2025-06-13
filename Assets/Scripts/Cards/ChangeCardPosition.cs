using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class ChangeCardPosition : MonoBehaviour
{
    Tilemap tilemap; // Asigna tu tilemap desde el inspector
    Tilemap obstacleTilemap; // Tilemap de paredes y obstáculos
    bool estaArrastrando = false;

    public LayerMask obstacleLayers; // Capas de obstáculos (paredes, etc.)
    public LayerMask spawnPointLayers;
    
    public float checkRadius = 0.1f; // Radio para verificación de colisión
    PlayerController player;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tilemap = GameObject.Find("Tilemap Laberinto").GetComponent<Tilemap>();
        obstacleTilemap = GameObject.Find("Paredes").GetComponent<Tilemap>();
        player = FindFirstObjectByType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (estaArrastrando)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0; // Para 2D

            Vector3Int cellPos = tilemap.WorldToCell(mousePos);
            if (IsPositionValid(cellPos))
            {
                Vector3 alignedPos = tilemap.GetCellCenterWorld(cellPos);
                transform.position = alignedPos;
            }
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


    void OnMouseDown()
    {
        if (!FindAnyObjectByType<GameManager>().inPrepPhase) return;
        // Empieza el arrastre
        estaArrastrando = true;
    }



    void OnMouseUp()
    {
        if (!FindAnyObjectByType<GameManager>().inPrepPhase) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = tilemap.WorldToCell(worldPos);

        if (IsPositionValid(cellPos))
        {
            estaArrastrando = false;

            // 1. Convertir posición del mouse a celda del tilemap
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z); // Z = 10
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
            mouseWorldPos.z = 0; // Asegurar Z = 0

            // 2. Obtener celda y posición central del tilemap
            Vector3Int cellPosition = tilemap.WorldToCell(mouseWorldPos);
            Vector3 finalPosition = tilemap.GetCellCenterWorld(cellPosition);
            finalPosition.z = 0; // Forzar Z del objeto igual al tilemap

            // 3. Actualizar posición
            transform.position = finalPosition;
        }


    }
}
