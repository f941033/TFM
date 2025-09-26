using UnityEngine;

[CreateAssetMenu(fileName = "New Summon Card", menuName = "Cards/Summon Card")]
public class SummonCardData : CardData
{
    public GameObject minionPrefab;
    public int numberOfMinions;
    public float cost;
    public int minionDamage;
    public int minionHealth;
    public int minionRange;

    public override void Play(PlayerController player, Vector3 worldPosition)
    {
        float spacing = 1.0f; // distancia entre minions en unidades de mundo (ajústalo a tu tile size)

        for (int i = 0; i < numberOfMinions; i++)
        {
            // i=0 aparece en worldPosition; i=1 a 1 unidad a la izquierda; etc.
            Vector3 spawnPos = worldPosition + Vector3.left * i * spacing;
            spawnPos.z = 0f; // plano 2D

            GameObject minion = Instantiate(minionPrefab, spawnPos, Quaternion.identity);
            // minion.GetComponent<MinionController>()?.Initialize(player);
        }
    }
}