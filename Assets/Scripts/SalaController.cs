using UnityEngine;
using UnityEngine.Tilemaps;

public class SalaController : MonoBehaviour
{
    public Tilemap tilemap;
    Color originalColor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalColor = tilemap.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        tilemap.gameObject.SetActive(false);
    }

    private void OnMouseEnter()
    {
        
        tilemap.color = Color.yellow;
    }

    private void OnMouseExit()
    {
        tilemap.color = originalColor;
    }
}
