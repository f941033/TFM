using System.Linq;
using UnityEngine;

public class RoomsManager : MonoBehaviour
{
    GameObject[] salas;
    int indexSala = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        salas = GameObject.FindGameObjectsWithTag("ColliderSala");
        salas = salas.OrderBy(sala => sala.transform.GetSiblingIndex()).ToArray();
        indexSala++;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenRing()
    {
        var rooms = salas[indexSala].GetComponentsInChildren<SalaController>();
        foreach (SalaController room in rooms )
        {
            room.OpenRoom();
        }
        indexSala++;
    }
}
