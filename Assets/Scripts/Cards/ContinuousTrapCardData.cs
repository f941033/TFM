using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Cards/ContiniousTrapCardData")]
public class ContinuousTrapCardData : TrapCardData
{
    [Header("Cloud Settings")]
    public float duration;
    public float radius;
    private void OnEnable() => cardType = CardType.Trap;

    public override void Play(PlayerController player, Vector3 worldPosition)
    {
        var trap = Instantiate(trapPrefab, worldPosition, Quaternion.identity);
        ContinuousTrapController ContTrapCont = trap.AddComponent<ContinuousTrapController>();
        ContTrapCont.Initialize(player, this);
    }
    public override void OnTrigger(PlayerController player, EnemyController enemy)
    {
        // No lo usamos aquí, toda la lógica está en la corrutina
    }
}
