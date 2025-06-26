using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Cards/Hability")]
public class HabilityCardData : CardData
{
    public GameObject areaPreviewPrefab;

    [Header("Daño")]
    public bool applyDamage = true;
    public int damage = 1;

    [Header("Debuff")]
    public bool applyDebuff = false;
    public float debuffDuration = 2f;

    [Tooltip("Lista de debuffs aplicados con sus multiplicadores")]
    public List<DebuffEffect> debuffs = new List<DebuffEffect>();

    [Header("Rango")]
    public float effectRadius = 5f;
    public LayerMask enemyLayer;

    private void OnEnable() => cardType = CardType.Hability;

    public override void Play(PlayerController player, Vector3 worldPosition)
    {
        ShowAreaEffect(worldPosition, effectRadius);
        var enemies = Physics2D.OverlapCircleAll(worldPosition, effectRadius, enemyLayer);
        Debug.Log("he jugado la carta de habilidad");
        foreach (var enemy in enemies)
        {
            var e = enemy.GetComponent<EnemyController>();
            if (e == null) continue;
            Debug.Log("He encontrado un enemigo");
            if (applyDamage)
                e.ReceiveDamage(damage);

            if (applyDebuff)
                e.ApplyDebuff(this);
        }
    }

    private void ShowAreaEffect(Vector3 center, float radius)
    {
        if (areaPreviewPrefab == null) return;

        GameObject preview = Instantiate(areaPreviewPrefab, center, Quaternion.identity);

        // Asumimos que el prefab tiene un círculo unitario (1x1), así que lo escalamos
        preview.transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);

        Destroy(preview, 3f); // desaparece tras 1 segundo
    }
}