using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SalaPolygonController : MonoBehaviour
{
    public Tilemap tilemap;
    public Tilemap borderTilemap;
    Color originalColor;
    public bool estaLibre = false;
    public GameObject[] salasContiguas;
    public GameObject[] wayPoints;
    public GameObject[] removeWayPoints;
    public SpawnEnemies spawnEnemiesController;
    public GameManager gm;


    GameObject parent;
    MerchantUI merchant;
    GameObject panelInfo;
    public List<FixedTrapBehaviour> trapsInRoom = new List<FixedTrapBehaviour>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        merchant = FindAnyObjectByType<MerchantUI>();
        parent = transform.parent.gameObject;
        originalColor = tilemap.color;

        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag("CanvasInfo"))
            {
                panelInfo = t.gameObject;
                break;
            }
        }
        if (panelInfo != null)
        {
            TextMeshProUGUI tmp = panelInfo.GetComponentInChildren<TextMeshProUGUI>(true); // El parámetro true busca aunque el objeto esté inactivo.
            if (tmp != null) tmp.text = "x" + trapsInRoom.Count;
        }



        if (gameObject.tag == "salaCentral")
        {
            estaLibre = true;
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseDown()
    {
        if (gameObject.tag == "salaCentral") return;

        if (panelInfo != null) panelInfo.gameObject.SetActive(false);

        if (gm.hasKey && esSalaContigua())
        {
            gm.hasKey = false;
            merchant.DeactiveKey();

            foreach (var item in parent.GetComponentsInChildren<SalaPolygonController>())
            {
                item.estaLibre = true;
                spawnEnemiesController.AddWayPoints(item.wayPoints, item.removeWayPoints);
                item.tilemap.gameObject.SetActive(false);
            }
            ActivarTrampas();
            gm.PreparationPhase();
        }

    }

    void ActivarTrampas()
    {
        foreach (var item in trapsInRoom)
        {
            item.ActivateAnimation();
        }
    }

    private void OnMouseEnter()
    {
        if (gameObject.tag == "salaCentral") return;

        if (!gm.hasKey && esSalaContigua() && !estaLibre)
        {
            borderTilemap.gameObject.SetActive(true);
            if (panelInfo != null) panelInfo.gameObject.SetActive(true);
        }

        if (gm.hasKey && esSalaContigua())
        {
            if (panelInfo != null) panelInfo.gameObject.SetActive(true);

            foreach (var item in parent.GetComponentsInChildren<SalaPolygonController>())
            {
                item.tilemap.color = Color.yellow;
            }
        }



    }

    private void OnMouseExit()
    {
        if (gameObject.tag == "salaCentral") return;

        if (panelInfo != null) panelInfo.gameObject.SetActive(false);
        borderTilemap.gameObject.SetActive(false);

        foreach (var item in parent.GetComponentsInChildren<SalaPolygonController>())
        {
            item.tilemap.color = originalColor;
        }
    }


    bool esSalaContigua()
    {
        if (salasContiguas == null) return false;
        foreach (GameObject sala in salasContiguas)
        {
            if (sala.GetComponent<SalaController>() == null)
            {
                if (sala.GetComponent<SalaPolygonController>().estaLibre) return true;
            }
            else
                if (sala.GetComponent<SalaController>().estaLibre) return true;

        }
        return false;
    }

    public int CountTrapsInRoom()
    {
        int trapCount = 0;
        Collider2D roomCollider = GetComponent<Collider2D>();
        GameObject[] traps = GameObject.FindGameObjectsWithTag("FixedTrap");

        foreach (GameObject trap in traps)
        {
            if (trap == null) continue;
            Vector2 pos = trap.transform.position;
            if (roomCollider.OverlapPoint(pos))
            {
                trapsInRoom.Add(trap.GetComponent<FixedTrapBehaviour>());
                trapCount++;
            }
        }
        return trapCount;
    }

    
}
