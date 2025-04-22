using UnityEngine;
using DeckboundDungeon.Cards;
public class TrapController : MonoBehaviour
{
    [HideInInspector] public TrapCardData cardData;
    [HideInInspector] public PlayerController player;
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("He entrado en el trigger de TrapController");
        if (!other.CompareTag("Enemy")) return;

        var enemy = other.GetComponent<EnemyController>();
        if (enemy == null) return;

        // Le decimos a la carta que aplique su lógica de trigger
        cardData.OnTrigger(player, enemy);
        Debug.Log("Ha encontrado un enemigo");

        if (cardData.used)
            Destroy(gameObject);

    }

    void Awake()
    {
        // Debug inicial para ver si el script se ejecuta
        Debug.Log($"[TrapController] Awake en «{gameObject.name}»", this);

        // Comprueba si tienes un BoxCollider2D
        var bc = GetComponent<BoxCollider2D>();
        Debug.Log($"[TrapController] BoxCollider2D: {(bc != null ? "OK" : "MISSING")}", this);
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