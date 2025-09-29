using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Cards/Hability")]
public class HabilityCardData : CardData
{
    [Header("Refs")]
    private Tilemap targetTilemap;     // <- referencia al Tilemap (asísnala en el asset o por código)
    public GameObject areaPreviewPrefab;

    [Header("Cooldown")]
    public float cooldown = 0f;

    [Header("Daño / Debuff")]
    public bool applyDamage = true;
    public int damage = 1;
    [SerializeField] private bool isAreaDamage;
    public bool IsDamage => applyDamage;
    public bool IsAreaDamage => isAreaDamage;

    public bool applyDebuff = false;
    public float debuffDuration = 2f;
    public List<DebuffEffect> debuffs = new List<DebuffEffect>();

    [Header("Área en cuadrícula (izq/arriba)")]
    [Min(1)] public int squareSize = 1;        // 1 = solo celda base; 2 = 2x2 (base, izq, arriba, arriba-izq)
    public bool requireTileToAffect = true;     // afecta solo si hay un tile pintado en esa celda

    [Header("Detección de enemigos")]
    public LayerMask enemyLayer;
    [Range(0.2f, 1.0f)] public float hitBoxScale = 0.95f; // tamaño del box dentro de la celda

    private void OnEnable() => cardType = CardType.Hability;

    public override void Play(PlayerController player, Vector3 worldPosition)
    {
        targetTilemap = GameObject.Find("Tilemap Laberinto").GetComponent<Tilemap>();
        if (targetTilemap == null) { Debug.LogWarning("[HabilityCardData] Falta asignar targetTilemap."); return; }

        Vector3Int baseCell = targetTilemap.WorldToCell(worldPosition);
        var cells = GetCellsLeftUp(baseCell, squareSize);

        ShowAreaBlockPreview(baseCell, squareSize);

        var processed = new HashSet<EnemyController>();
        Vector2 cellSize = targetTilemap.cellSize;
        Vector2 boxSize = cellSize * hitBoxScale;

        foreach (var cell in cells)
        {
            if (requireTileToAffect && !targetTilemap.HasTile(cell)) continue;

            Vector3 center = targetTilemap.GetCellCenterWorld(cell);
            var hits = Physics2D.OverlapBoxAll(center, boxSize, 0f, enemyLayer);
            foreach (var h in hits)
            {
                var e = h.GetComponent<EnemyController>();
                if (e == null || processed.Contains(e)) continue;

                if (applyDamage) e.ReceiveDamage(damage);
                if (applyDebuff) e.ApplyDebuff(this);
                processed.Add(e);
            }
        }
    }

    private void ShowAreaBlockPreview(Vector3Int baseCell, int size)
    {
        if (areaPreviewPrefab == null) return;

        Vector2 cell = targetTilemap.cellSize;
        Vector3 baseCenter = targetTilemap.GetCellCenterWorld(baseCell);

        float offX = (size - 1) * 0.5f * cell.x;
        float offY = (size - 1) * 0.5f * cell.y;
        Vector3 areaCenter = baseCenter + new Vector3(-offX, +offY, 0f);

        // tamaño total del bloque en mundo
        Vector2 total = new Vector2(cell.x * size, cell.y * size);

        var go = Object.Instantiate(areaPreviewPrefab, areaCenter, Quaternion.identity);

        // 🔧 escalar en mundo al tamaño deseado
        ScalePrefabToWorldSize(go, total);

        Object.Destroy(go, 1.5f);
    }

    private static List<Vector3Int> GetCellsLeftUp(Vector3Int baseCell, int size)
    {
        var list = new List<Vector3Int>(size * size);
        for (int dx = 0; dx < size; dx++)       // 0 = base, 1 = 1 a la izquierda...
        {
            for (int dy = 0; dy < size; dy++)   // 0 = base, 1 = 1 arriba...
            {
                list.Add(new Vector3Int(baseCell.x - dx, baseCell.y + dy, baseCell.z));
            }
        }
        return list;
    }

    static void ScalePrefabToWorldSize(GameObject go, Vector2 targetWorldSize)
    {
        var rend = go.GetComponentInChildren<Renderer>();
        if (rend == null) return;

        // tamaño actual del prefab en mundo (con su escala actual)
        Vector3 cur = rend.bounds.size;
        if (cur.x <= 0f || cur.y <= 0f) return;

        // escala actual del objeto que vas a tocar
        Vector3 s = go.transform.localScale;

        // nueva escala necesaria para alcanzar el tamaño objetivo
        float sx = targetWorldSize.x / cur.x * s.x;
        float sy = targetWorldSize.y / cur.y * s.y;

        go.transform.localScale = new Vector3(sx, sy, s.z);
    }
}
