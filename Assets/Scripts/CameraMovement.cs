using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Límites del Laberinto")]
    public float xMin = -10f;
    public float xMax = 10f;
    public float yMin = -10f;
    public float yMax = 10f;

    [Header("Configuración de Movimiento")]
    public float dragSpeed = 2f;

    [Header("Configuración de Zoom")]
    public float zoomSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 20f;

    private Camera mainCamera;
    private Vector3 dragOrigin;
    private Vector3 lastMousePosition;
    private bool isDragging = false;
    float cameraZOffset = -30f; // Mantener la cámara en esta profundidad


    void Start()
    {
        // Obtener la cámara principal
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = GetComponent<Camera>();

        if (mainCamera == null)
        {
            Debug.LogError("No se pudo encontrar el componente Camera. Asegúrate de que este script esté en un objeto con una cámara.");
        }

        // Asegurar que la cámara inicie en el centro del mapa donde está el player
        Vector3 playerPosition = FindAnyObjectByType<PlayerController>().transform.position;
        transform.position = new Vector3(playerPosition.x, playerPosition.y, cameraZOffset);
    }

    void Update()
    {
        HandleMouseDrag();
        HandleZoom();
    }

    void HandleMouseDrag()
    {
        // Detectar cuando se presiona la rueda del ratón (botón medio)
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }

        // Mientras se mantiene presionada la rueda del ratón
        if (Input.GetMouseButton(2) && isDragging)
        {
            Vector3 currentMousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 difference = dragOrigin - currentMousePosition;

            // Calcular la nueva posición
            Vector3 targetPosition = transform.position + difference;

            // Aplicar límites de movimiento
            targetPosition = ApplyCameraBounds(targetPosition);

            // Aplicar la posición con los límites
            transform.position = targetPosition;
        }

        // Cuando se suelta la rueda del ratón
        if (Input.GetMouseButtonUp(2))
        {
            isDragging = false;
        }
    }

    void HandleZoom()
    {
        // Obtener el input de la rueda del ratón para hacer zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f && mainCamera != null)
        {
            // Para cámara ortográfica
            if (mainCamera.orthographic)
            {
                mainCamera.orthographicSize -= scroll * zoomSpeed;
                mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minZoom, maxZoom);
            }
            // Para cámara en perspectiva
            else
            {
                mainCamera.fieldOfView -= scroll * zoomSpeed * 10f;
                mainCamera.fieldOfView = Mathf.Clamp(mainCamera.fieldOfView, minZoom * 3f, maxZoom * 3f);
            }

            // Después del zoom, verificar que la cámara siga dentro de los límites
            transform.position = ApplyCameraBounds(transform.position);
        }
    }

    Vector3 ApplyCameraBounds(Vector3 targetPosition)
    {
        if (mainCamera == null) return targetPosition;

        // Calcular el tamaño visible de la cámara
        float camHeight, camWidth;

        if (mainCamera.orthographic)
        {
            // Para cámara ortográfica
            camHeight = mainCamera.orthographicSize;
            camWidth = camHeight * mainCamera.aspect;
        }
        else
        {
            // Para cámara en perspectiva (aproximación)
            float distance = Mathf.Abs(targetPosition.z);
            camHeight = distance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            camWidth = camHeight * mainCamera.aspect;
        }

        // Aplicar los límites considerando el tamaño de la cámara
        float limitedX = Mathf.Clamp(targetPosition.x, xMin + camWidth, xMax - camWidth);
        float limitedY = Mathf.Clamp(targetPosition.y, yMin + camHeight, yMax - camHeight);

        // Mantener la posición Z original
        return new Vector3(limitedX, limitedY, targetPosition.z);
    }

    // Método para ajustar los límites dinámicamente si es necesario
    public void SetBounds(float newXMin, float newXMax, float newYMin, float newYMax)
    {
        xMin = newXMin;
        xMax = newXMax;
        yMin = newYMin;
        yMax = newYMax;

        // Verificar inmediatamente si la cámara está dentro de los nuevos límites
        transform.position = ApplyCameraBounds(transform.position);
    }

    // Método para visualizar los límites en el editor (opcional)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = new Vector3((xMin + xMax) / 2f, (yMin + yMax) / 2f, transform.position.z);
        Vector3 size = new Vector3(xMax - xMin, yMax - yMin, 0.1f);
        Gizmos.DrawWireCube(center, size);
    }
}