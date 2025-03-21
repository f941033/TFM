using UnityEngine;
using UnityEngine.Tilemaps;

public class SalaController : MonoBehaviour
{
    public Tilemap tilemap;
    Color originalColor;
    public bool estaLibre = false;
    public GameObject[] salasContiguas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalColor = tilemap.color;
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
        if (esSalaContigua())
        {
            tilemap.gameObject.SetActive(false);
            estaLibre = true;
        }
            
    }

    private void OnMouseEnter()
    {
        if (esSalaContigua())
        {
            tilemap.color = Color.yellow;
            
        }
        
    }

    private void OnMouseExit()
    {
        tilemap.color = originalColor;
    }

    bool esSalaContigua()
    {
        foreach(GameObject sala in salasContiguas)
        {
            if(sala.GetComponent<SalaController>().estaLibre)
            {
                return true;
            }
        }
        return false;
    }
}
