using System.Collections.Generic;
using UnityEngine;

public class TeslaTowerController : MonoBehaviour
{
    [Header("Parámetros Tesla")]
    public float tickRate = 1f;
    public float attackRadius = 2f;
    public float areaDamage = 10f;

    [Header("Sinergia entre Torres")]
    public float connectionRadius = 10f;
    public float rayDamage = 20f;
    public LayerMask enemyLayer;
    public Material rayMaterial;

    private float tickTimer = 0f;
    private AudioSource audioSource;

    private List<TeslaTowerController> connectedTowers = new();
    private List<LineRenderer> rayLines = new();

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        TryFindConnections();
    }

    private void Update()
    {
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickRate)
        {
            tickTimer = 0f;
            AreaAttack();

            for (int i = 0; i < connectedTowers.Count; i++)
            {
                TeslaTowerController other = connectedTowers[i];
                if (other == null) continue;

                RayAttack(other);
                UpdateLine(i, other);
            }
        }
    }

    void AreaAttack()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, attackRadius, enemyLayer);
        foreach (var enemy in enemies)
        {
            var enemyCtrl = enemy.GetComponent<EnemyController>();
            if (enemyCtrl != null)
            {
                enemyCtrl.ReceiveDamage(areaDamage);
            }
        }
    }

    void RayAttack(TeslaTowerController other)
    {
        Vector3 dir = other.transform.position - transform.position;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dir.normalized, dir.magnitude, enemyLayer);

        foreach (var hit in hits)
        {
            var enemyCtrl = hit.collider.GetComponent<EnemyController>();
            if (enemyCtrl != null)
            {
                enemyCtrl.ReceiveDamage(rayDamage);
            }
        }
    }

    void TryFindConnections()
    {
        TeslaTowerController[] allTowers = FindObjectsByType<TeslaTowerController>(FindObjectsSortMode.None);

        foreach (var other in allTowers)
        {
            if (other == this || connectedTowers.Contains(other)) continue;
            if (connectedTowers.Count >= 2) break;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist <= connectionRadius && other.CanConnectTo(this))
            {
                ConnectWith(other);
            }
        }
    }

    bool CanConnectTo(TeslaTowerController other)
    {
        return !connectedTowers.Contains(other) && connectedTowers.Count < 2;
    }

    void ConnectWith(TeslaTowerController other)
    {
        connectedTowers.Add(other);
        other.connectedTowers.Add(this);

        var line = CreateLine();
        rayLines.Add(line);

        var otherLine = other.CreateLine();
        other.rayLines.Add(otherLine);
    }

    LineRenderer CreateLine()
    {
        var lineObj = new GameObject("TeslaRay");
        lineObj.transform.parent = this.transform;

        var line = lineObj.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.material = rayMaterial != null ? rayMaterial : new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.cyan;
        line.endColor = Color.cyan;

        return line;
    }

    void UpdateLine(int index, TeslaTowerController other)
    {
        if (index < rayLines.Count)
        {
            rayLines[index].SetPosition(0, transform.position);
            rayLines[index].SetPosition(1, other.transform.position);
        }
    }

    private void OnDestroy()
    {
        foreach (var other in connectedTowers)
        {
            if (other != null)
                other.RemoveConnectionWith(this);
        }
        connectedTowers.Clear();

        foreach (var line in rayLines)
        {
            if (line != null)
                Destroy(line.gameObject);
        }
        rayLines.Clear();
    }

    public void RemoveConnectionWith(TeslaTowerController other)
    {
        int index = connectedTowers.IndexOf(other);
        if (index >= 0)
        {
            connectedTowers.RemoveAt(index);
            if (index < rayLines.Count)
            {
                Destroy(rayLines[index].gameObject);
                rayLines.RemoveAt(index);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, connectionRadius);
    }
}
