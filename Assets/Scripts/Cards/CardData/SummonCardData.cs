using UnityEngine;

[CreateAssetMenu(fileName = "New Summon Card", menuName = "Cards/Summon Card")]
public class SummonCardData : CardData
{
    public GameObject minionPrefab;
    public int numberOfMinions;
    public float cost;

    public override void Play(PlayerController player, Vector3 worldPosition)
    {
        for (int i = 0; i < numberOfMinions; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 1f;
            Vector3 spawnPos = worldPosition + (Vector3)offset;
            spawnPos.z = 0; // asegurar plano 2D

            GameObject minion = GameObject.Instantiate(minionPrefab, spawnPos, Quaternion.identity);
            // puedes pasarle el jugador si es necesario:
            // minion.GetComponent<MinionController>()?.Initialize(player);
        }
    }
}