using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SalaController : MonoBehaviour
{
    public Tilemap tilemap;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        merchant = FindAnyObjectByType<MerchantUI>();
        parent = transform.parent.gameObject;
        originalColor = tilemap.color;

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

            gm.PreparationPhase();
            //indexSala++;
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

        if (gm.hasKey && esSalaContigua())
        {
            foreach (var item in parent.GetComponentsInChildren<SalaController>())
            {
                item.tilemap.color = Color.yellow;
            }
        }

    }

    private void OnMouseExit()
    {
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
}
