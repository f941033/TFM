using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using DeckboundDungeon.Cards;

public class ContinuousTrapController : MonoBehaviour
{
    Tilemap zone;
    Vector3Int cell;
    Color originalColor;
    float duration, radius, damage;
    private TileBase originalTile;
    PlayerController player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalTile = Resources.Load<TileBase>("Tiles/FloorTile");
    }

    public void Initialize(PlayerController player, ContinuousTrapCardData cardData)
    {
        this.player = player;
        this.duration = cardData.duration;
        this.radius = cardData.radius;
        this.damage = cardData.damage;

        CardManager cardMan = FindFirstObjectByType<CardManager>();
        zone = cardMan.zonaValidaTilemap;
        cell = zone.WorldToCell(transform.position);
        originalColor = zone.GetColor(cell);

        zone.SetTileFlags(cell, TileFlags.None);
        zone.SetColor(cell, Color.cyan);

        StartCoroutine(CloudDamage());
    }

    IEnumerator CloudDamage()
    {
        float elapsed = 0f;
        while(elapsed < duration)
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach(var hit in hits)
            {
                if(hit.CompareTag("Enemy")){
                    Debug.Log("He detectado un enemigo, le hago daño");
                    hit.GetComponent<EnemyController>().receiveDamage(damage * Time.deltaTime);
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
        zone.SetTileFlags(cell, TileFlags.LockColor);
        zone.SetColor(cell, originalColor);
        zone.SetTile(cell, originalTile);
        Destroy(gameObject);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
