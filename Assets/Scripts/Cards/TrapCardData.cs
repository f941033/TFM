using UnityEngine;

[CreateAssetMenu(menuName = "Cards/TrapCard")]
public class TrapCardData : CardData
{
    [Header("Trap")]
    public GameObject trapPrefab;
    public Vector3 spawnOffset;

    private void OnEnable() { cardType = CardType.Trap; }

    public override void Play(PlayerController player)
    {
        Vector3 pos = player.transform.position + spawnOffset;
        Instantiate(trapPrefab, pos, Quaternion.identity);
    }
}
