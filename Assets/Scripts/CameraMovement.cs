
using System.Collections;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
     //float moveSpeed = 10f;
     //float borderThickness = 0.5f;
     Vector2 mapSize = new Vector2(55f, 55f); // Tamaño del mapa
     float cameraZOffset = -30f; // Mantener la cámara en esta profundidad

    private Camera mainCamera;
    private float cameraHeight;
    private float cameraWidth;

    private float zoomSpeed = 5f;
    private float minZoom = 4f;
    private float maxZoom = 25f;

    private Vector3 dragOriginMaze;
    private bool isDraggingMaze = false;

    void Start()
    {

        mainCamera = Camera.main;
        // Calcular el tamaño de la cámara en el mundo (importante hacerlo en Start)
        cameraHeight = 2f * mainCamera.orthographicSize;
        cameraWidth = cameraHeight * mainCamera.aspect;

        // Asegurar que la cámara inicie en el centro del mapa (o donde se desee)
        transform.position = new Vector3(mapSize.x / 2f, mapSize.y / 2f, cameraZOffset);
    }

    void Update()
    {
        /* --------------------------------------------
         *              ZOOM CON RUEDA
         * --------------------------------------------*/             
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            mainCamera.orthographicSize -= scroll * zoomSpeed;
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minZoom, maxZoom);
        }



        /*--------------------------------------------
         *         ARRASTRE DEL NIVEL CON RUEDA
         *--------------------------------------------*/

        // Detectar inicio del arrastre con la rueda del ratón (botón 2)
        if (Input.GetMouseButtonDown(2))
        {
            dragOriginMaze = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isDraggingMaze = true;
        }

        // Mientras se mantenga presionado el botón central
        if (isDraggingMaze && Input.GetMouseButton(2))
        {
            Vector3 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 difference = dragOriginMaze - currentPos;
            transform.position += difference;
        }

        // Al soltar la rueda del ratón
        if (Input.GetMouseButtonUp(2))
        {
            isDraggingMaze = false;
        }
    }



    //-------------- EL MOVIMIENTO DE CÁMARA YA NO SE USA -----------------
 
    //public void StartCameraMovement()
    //{
    //    Debug.Log("arrancando movimiento de cámara");
    //    //StartCoroutine("MoveCamera");
    //}

    //public void StopCameraMovement()
    //{
    //    Debug.Log("parando movimiento de cámara");
    //    StopCoroutine("MoveCamera");
    //}


    //IEnumerator MoveCamera()
    //{
    //    while (true)
    //    {
    //        Vector3 mousePos = Input.mousePosition;
    //        Vector3 cameraPos = transform.position;

    //        // Movimiento Horizontal
    //        if (mousePos.x >= Screen.width - borderThickness)
    //        {
    //            cameraPos.x += moveSpeed * Time.deltaTime;
    //        }
    //        else if (mousePos.x <= borderThickness)
    //        {
    //            cameraPos.x -= moveSpeed * Time.deltaTime;
    //        }

    //        // Movimiento Vertical
    //        if (mousePos.y >= Screen.height - borderThickness)
    //        {
    //            cameraPos.y += moveSpeed * Time.deltaTime;
    //        }
    //        else if (mousePos.y <= borderThickness)
    //        {
    //            cameraPos.y -= moveSpeed * Time.deltaTime;
    //        }

    //        // Limitar la posición de la cámara al tamaño del mapa
    //        // Aquí es CLAVE restar la mitad del tamaño de la cámara para centrarla
    //        cameraPos.x = Mathf.Clamp(cameraPos.x, 0f + cameraWidth / 2, mapSize.x - cameraWidth / 2);
    //        cameraPos.y = Mathf.Clamp(cameraPos.y, 0f + cameraHeight / 2, mapSize.y - cameraHeight / 2);

    //        transform.position = new Vector3(cameraPos.x, cameraPos.y, cameraZOffset);
    //        yield return null;
    //    }
    //}


    
}
