using UnityEngine;
using UnityEngine.Tilemaps;

public class ChangeCardPosition : MonoBehaviour
{
    Tilemap tilemap; // Asigna tu tilemap desde el inspector
    bool estaArrastrando = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tilemap = FindFirstObjectByType<Tilemap>();
    }

    // Update is called once per frame
    void Update()
    {
        if (estaArrastrando)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0; // Para 2D
            transform.position = mousePos;
        }
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
