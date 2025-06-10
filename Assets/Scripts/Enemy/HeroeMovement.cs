using UnityEngine;
using System.Collections;

public class HeroeMovement : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("detectando muro exterior");

        if (collision.gameObject.tag == "PlayerWall")
        {
            Debug.Log("detectando muro");
            Invoke("Destruir", 2f);
        }
    }

    void Destruir()
    {
        Debug.Log("muro destruido");
    }
}
