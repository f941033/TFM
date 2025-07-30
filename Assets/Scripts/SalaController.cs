using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SalaController : MonoBehaviour
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

    //static GameObject[] salas;
    //static int indexSala = 0;
    GameObject parent;
    MerchantUI merchant;
    GameObject panelInfo;
    List<FixedTrapBehaviour> trapsInRoom = new List<FixedTrapBehaviour>();

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
            if (tmp != null) tmp.text = "x" + CountTrapsInRoom();
        }



        if (gameObject.tag == "salaCentral")
        {
            estaLibre = true;
        }

        //if (salas != null) return;
        //else
        //{
        //    salas = GameObject.FindGameObjectsWithTag("ColliderSala");
        //    salas = salas.OrderBy(sala => sala.transform.GetSiblingIndex()).ToArray();
        //    indexSala++;
        //}

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

            foreach (var item in parent.GetComponentsInChildren<SalaController>())
            {
                item.estaLibre = true;
                spawnEnemiesController.AddWayPoints(item.wayPoints, item.removeWayPoints);
                item.tilemap.gameObject.SetActive(false);
            }
            ActivarTrampas();
            gm.PreparationPhase();
            //indexSala++;
        }

    }

    void ActivarTrampas()
    {
        foreach (var item in trapsInRoom)
        {
            item.ActivateAnimation();
        }
    }


    //public void OpenRoom()
    //{
    //    estaLibre = true;
    //    spawnEnemiesController.AddWayPoints(wayPoints, removeWayPoints);
    //    tilemap.gameObject.SetActive(false);
    //}

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

            foreach (var item in parent.GetComponentsInChildren<SalaController>())
            {
                item.tilemap.color = Color.yellow;
            }
        }

        

    }

    private void OnMouseExit()
    {
        if (gameObject.tag == "salaCentral") return;

        if(panelInfo !=null) panelInfo.gameObject.SetActive(false);
        borderTilemap.gameObject.SetActive(false);

        foreach (var item in parent.GetComponentsInChildren<SalaController>())
        {
            item.tilemap.color = originalColor;
        }
    }


    bool esSalaContigua()
    {
        if (salasContiguas == null) return false;
        foreach (GameObject sala in salasContiguas)
        {
            if (sala.GetComponent<SalaController>().estaLibre)
            {
                return true;
            }
        }
        return false;
    }

    //public void TengoCartaAbrirSala()
    //{
    //    cartaAbrirSala = true;
    //}

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
