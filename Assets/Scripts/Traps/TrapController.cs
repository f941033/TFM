using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using DeckboundDungeon.Cards;
using System.Collections;

public class TrapController : MonoBehaviour
{
    [HideInInspector] public TrapCardData cardData;
    [HideInInspector] public PlayerController player;

    [HideInInspector] public Vector3Int cellPos;
    [HideInInspector] public Color initialColor;
    private TileBase originalTile;
    private Tilemap tilemap;
    void Awake()
    {
        tilemap = FindFirstObjectByType<CardManager>()?.zonaValidaTilemap;
        if (tilemap == null)
            Debug.LogError("[TrapController] no encontré zonaValidaTilemap", this);

    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!(cardData is InstantTrapCardData)) return;
        if (!other.CompareTag("Enemy")) return;

        Debug.Log("Algo ha entrado en el collider");
        var enemy = other.GetComponent<EnemyController>();
        if (enemy == null) return;

        cardData.OnTrigger(player, enemy);

        if (cardData.used)
            ClearAndDestroy();
    }

    void Start()
    {
        originalTile = Resources.Load<TileBase>("Tiles/FloorTile");
        //if (cardData is ContinuousTrapCardData cloudData)
        //{
            //Debug.Log("Entro en que es de tipo Continuo");
            //StartCoroutine(CloudDamage(cloudData));
            //return;
        //}
        //Debug.Log("Y ahora estoy fuera del if");
    }

    private IEnumerator CloudDamage(ContinuousTrapCardData cloud)
    {
        float elapsed = 0f;
        while(elapsed < cloud.duration)
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, cloud.radius);
            foreach(var hit in hits)
            {
                if(hit.CompareTag("Enemy"))
                    hit.GetComponent<EnemyController>().receiveDamage(cloud.damage * Time.deltaTime);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
        ClearAndDestroy();
    }

    private void ClearAndDestroy(){
        tilemap.SetTileFlags(cellPos, TileFlags.LockColor);
        tilemap.SetColor(cellPos, Color.white);
        tilemap.SetTile(cellPos, originalTile);
        Destroy(gameObject); 
    }
    void OnDrawGizmos()
    {
        // Marcador siempre visible en Scene View
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position, 0.1f);

        // Ahora intentamos dibujar cualquier Collider2D
        var cols = GetComponents<Collider2D>();
        foreach (var col in cols)
        {
            if (col is BoxCollider2D box)
            {
                Vector3 c = transform.position + (Vector3)box.offset;
                Vector3 s = new Vector3(box.size.x, box.size.y, 0);
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(c, s);
            }
            else if (col is CircleCollider2D circ)
            {
                Vector3 c = transform.position + (Vector3)circ.offset;
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(c, circ.radius);
            }
            // si hay otros Collider2D, añádelos aquí…
        }
    }
}