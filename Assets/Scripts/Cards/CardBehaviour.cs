using UnityEngine;
using UnityEngine.Tilemaps;

public class CardBehaviour : MonoBehaviour
{
    Tilemap tilemap;
    int enemiesKilled = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tilemap = FindAnyObjectByType<Tilemap>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy") 
        {
            enemiesKilled++;
            if (enemiesKilled == 3)
            {
                GetComponent<TrapController>().ClearAndDestroy();
            }

        }
    }
}
