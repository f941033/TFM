using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeslaTowerController : MonoBehaviour
{
    [Header("Detección / Targeting")]
    [SerializeField] float detectionRadius = 3f;
    [SerializeField] float checkInterval   = 0.2f;

    [Header("Daño al objetivo (single-target)")]
    [SerializeField] float tickRate     = 1f;   // cadencia (s)
    [SerializeField] float areaDamage   = 5f;   // se puede sobrescribir desde TrapCardData.damage
    [SerializeField] bool  drawTargetBeam = true;

    [Header("Conexión entre Teslas")]
    [SerializeField] float connectionRadius      = 3f;    // baja esto para enlazar a menos distancia
    [SerializeField] int   maxConnections        = 2;
    [SerializeField] bool  connectionDealsDamage = true;  // ← activa daño en el rayo de conexión
    [SerializeField] float connectionDamage      = 10f;   // daño por tic en el rayo
    [SerializeField] float connectionHitThickness= 0.25f; // grosor del “haz” para colisiones

    [Header("Render")]
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] Material  targetBeamMaterial;
    [SerializeField] Material  connectionBeamMaterial;

    [Header("Debug")]
    [SerializeField] bool debugLogs = false;

    // Runtime
    float tickTimer = 0f;
    List<Transform> enemiesInRange = new();
    Transform currentTarget;
    LineRenderer targetBeam;

    List<TeslaTowerController> connectedTowers = new();
    List<LineRenderer> connectionLines = new();

    void Awake()
    {
        // Si el prefab tiene TrapController con tu TrapCardData, toma su daño configurado
        var trap = GetComponent<TrapController>();
        if (trap != null && trap.cardData is TrapCardData td)
            areaDamage = td.damage;

        if (drawTargetBeam)
        {
            targetBeam = gameObject.AddComponent<LineRenderer>();
            targetBeam.enabled = false;
            targetBeam.positionCount = 2;
            targetBeam.material = targetBeamMaterial != null ? targetBeamMaterial : new Material(Shader.Find("Sprites/Default"));
            targetBeam.startWidth = 0.06f;
            targetBeam.endWidth   = 0.06f;
            targetBeam.startColor = Color.cyan;
            targetBeam.endColor   = Color.cyan;
            targetBeam.numCapVertices = 2;
        }
    }

    void Start()
    {
        StartCoroutine(EnemyDetectionRoutine());
        TryFindConnections();
        StartCoroutine(ReconnectRoutine());
    }

    IEnumerator EnemyDetectionRoutine()
    {
        while (true)
        {
            UpdateEnemiesInRange();
            yield return new WaitForSeconds(checkInterval);
        }
    }

    void UpdateEnemiesInRange()
    {
        int mask = (enemyLayer.value == 0) ? Physics2D.AllLayers : enemyLayer.value;
        var hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, mask);

        var now = new HashSet<Transform>();
        foreach (var h in hits)
        {
            if (h == null) continue;
            var t = h.transform;
            if (!t.CompareTag("Enemy")) continue;
            now.Add(t);
            if (!enemiesInRange.Contains(t))
                enemiesInRange.Add(t); // FIFO
        }
        enemiesInRange.RemoveAll(t => t == null || !now.Contains(t));

        if (enemiesInRange.Count > 0)
        {
            if (currentTarget == null || !enemiesInRange.Contains(currentTarget))
                currentTarget = enemiesInRange[0];
        }
        else currentTarget = null;
    }

    void Update()
    {
        if (currentTarget != null &&
            Vector2.Distance(transform.position, currentTarget.position) > detectionRadius)
            currentTarget = null;

        tickTimer += Time.deltaTime;
        if (tickTimer >= tickRate)
        {
            tickTimer = 0f;

            // Daño single-target
            if (currentTarget != null)
                DoTickDamageToTarget(currentTarget);

            // Daño en rayo de conexión (a todos los que pasen)
            if (connectionDealsDamage)
            {
                foreach (var other in connectedTowers)
                {
                    if (other == null) continue;
                    if (IsConnectionOwner(other)) // evitar doble aplicación
                        ConnectionAttack(other);
                }
            }
        }

        UpdateTargetBeam();
        UpdateConnectionLines();
    }

    void DoTickDamageToTarget(Transform target)
    {
        var enemy = target.GetComponentInParent<EnemyController>();
        if (enemy != null)
        {
            enemy.ReceiveDamage(areaDamage);
            if (debugLogs) Debug.Log($"[Tesla] Tick target {target.name} dmg:{areaDamage}", this);
        }
    }

    void UpdateTargetBeam()
    {
        if (targetBeam == null) return;
        if (currentTarget != null)
        {
            targetBeam.enabled = true;
            targetBeam.SetPosition(0, transform.position);
            targetBeam.SetPosition(1, currentTarget.position);
        }
        else targetBeam.enabled = false;
    }

    // ---------------- Conexión entre Teslas ----------------

    IEnumerator ReconnectRoutine()
    {
        var wait = new WaitForSeconds(0.5f);
        while (true)
        {
            if (connectedTowers.Count < maxConnections) TryFindConnections();
            yield return wait;
        }
    }

    void TryFindConnections()
    {
        var all = FindObjectsByType<TeslaTowerController>(FindObjectsSortMode.None);
        foreach (var other in all)
        {
            if (other == this) continue;
            if (connectedTowers.Contains(other)) continue;
            if (connectedTowers.Count >= maxConnections) break;
            if (!other.CanConnectTo(this)) continue;

            float d = Vector2.Distance(transform.position, other.transform.position);
            if (d <= connectionRadius)
            {
                ConnectWith(other);
                if (debugLogs) Debug.Log($"[Tesla] Conectada con {other.name}", this);
            }
        }
    }

    bool CanConnectTo(TeslaTowerController a)
        => !connectedTowers.Contains(a) && connectedTowers.Count < maxConnections;

    void ConnectWith(TeslaTowerController other)
    {
        connectedTowers.Add(other);
        other.connectedTowers.Add(this);

        connectionLines.Add(CreateConnectionLine());
        other.connectionLines.Add(other.CreateConnectionLine());
    }

    LineRenderer CreateConnectionLine()
    {
        var go = new GameObject("TeslaLink");
        go.transform.SetParent(transform, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.material = connectionBeamMaterial != null ? connectionBeamMaterial : new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = 0.05f;
        lr.endWidth   = 0.05f;
        lr.startColor = new Color(0f,1f,1f,0.85f);
        lr.endColor   = lr.startColor;
        lr.numCapVertices = 2;
        return lr;
    }

    void UpdateConnectionLines()
    {
        for (int i = 0; i < connectedTowers.Count && i < connectionLines.Count; i++)
        {
            var o = connectedTowers[i];
            if (o == null) continue;
            connectionLines[i].SetPosition(0, transform.position);
            connectionLines[i].SetPosition(1, o.transform.position);
        }
    }

    // --- Daño a lo que cruce el rayo de conexión (una sola vez por par) ---
    bool IsConnectionOwner(TeslaTowerController other)
        => GetInstanceID() < other.GetInstanceID(); // el de ID menor “posee” la conexión

    void ConnectionAttack(TeslaTowerController other)
    {
        Vector2 a = transform.position;
        Vector2 b = other.transform.position;
        Vector2 dir = (b - a).normalized;
        float dist = Vector2.Distance(a, b);

        // BoxCast “grueso” centrado en el segmento AB
        int mask = (enemyLayer.value == 0) ? Physics2D.AllLayers : enemyLayer.value;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Vector2 size = new Vector2(dist, connectionHitThickness * 2f);

        var hits = Physics2D.BoxCastAll((a + b) * 0.5f, size, angle, Vector2.zero, 0f, mask);

        int count = 0;
        foreach (var h in hits)
        {
            if (h.collider == null) continue;
            if (!h.collider.CompareTag("Enemy")) continue;

            var enemy = h.collider.GetComponentInParent<EnemyController>();
            if (enemy != null)
            {
                enemy.ReceiveDamage(connectionDamage);
                count++;
            }
        }

        if (debugLogs)
        {
            Debug.DrawLine(a, b, Color.magenta, tickRate * 0.9f);
            if (count > 0) Debug.Log($"[Tesla] Conn beam hit {count} enemies", this);
        }
    }

    // --------------- Limpieza ---------------
    void OnDestroy()
    {
        foreach (var o in connectedTowers)
            if (o != null) o.RemoveConnectionWith(this);

        foreach (var lr in connectionLines)
            if (lr != null) Destroy(lr.gameObject);

        connectedTowers.Clear();
        connectionLines.Clear();
    }

    public void RemoveConnectionWith(TeslaTowerController other)
    {
        int i = connectedTowers.IndexOf(other);
        if (i >= 0)
        {
            connectedTowers.RemoveAt(i);
            if (i < connectionLines.Count)
            {
                Destroy(connectionLines[i].gameObject);
                connectionLines.RemoveAt(i);
            }
        }
    }

    // Gizmos
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, connectionRadius);
    }
}
